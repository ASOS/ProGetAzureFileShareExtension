using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Inedo.NuGet;
using Inedo.NuGet.Packages;
using Inedo.ProGet.Data;
using Inedo.ProGet.Extensibility;
using Inedo.ProGet.Extensibility.PackageStores;
using RedDog.Storage.Files;

namespace ProGetAzureFileShareExtension
{
    /// <summary>
    /// Default implementation of a NuGet package store.
    /// </summary>
    [ProGetComponentProperties("Azure File Storage Nuget Package Store", "Uses an azure file share for the nuget package store")]
    public class AzureFileShareNuGetPackageStore : NuGetPackageStoreBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFileShareNuGetPackageStore"/> class.
        /// </summary>
        public AzureFileShareNuGetPackageStore()
        {
        }

        /// <summary>
        /// Gets the root file system directory of the package store.
        /// </summary>
        [Persistent]
        public string RootPath { get; protected set; }

        [Persistent]
        public string DriveLetter { get; protected set; }

        [Persistent]
        public string FileShareName { get; protected set; }

        [Persistent]
        public string UserName { get; protected set; }

        [Persistent]
        public string AccessKey { get; protected set; }


        private void InitPackageStore()
        {
            //todo: assert that DriveLetter is a single letter followed by a colon
            //todo: assert that RootPath starts with DriveLetter
            //todo: assert that UserName is not null
            //todo: assert that AccessKey is not null, (and looks like a base64 encoded key?)

            try
            {
                FilesMappedDrive.Mount(
                    DriveLetter, @"\\" + UserName + @".file.core.windows.net\" + FileShareName,
                    UserName, AccessKey);
                this.LogDebug("drive mapping successful.");
            }
            catch (Exception ex)
            {
                this.LogError("Exception occurred mapping drive - " + ex);
            }
        }

        /// <summary>
        /// Returns a stream backed by the specified package if it exists; otherwise returns null.
        /// </summary>
        /// <param name="packageId">The ID of the package.</param>
        /// <param name="packageVersion">The version of the package.</param>
        /// <returns>Stream backed by the specified package if it exists; otherwise null.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="packageId"/> is null or contains only whitespace or <paramref name="packageVersion"/> is null.</exception>
        public override Stream OpenPackage(string packageId, SemanticVersion packageVersion)
        {
            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentNullException("packageId");
            if (packageVersion == null)
                throw new ArgumentNullException("packageVersion");

            InitPackageStore();

            var packagePath = Path.Combine(this.RootPath, packageId);
            if (!Directory.Exists(packagePath))
                return null;

            var versionPath = Path.Combine(packagePath, packageId + "." + packageVersion + ".nupkg");
            if (!File.Exists(versionPath))
                return null;

            try
            {
                return new FileStream(versionPath, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
        }
        /// <summary>
        /// Returns an empty stream which can be used to write a new package to the store.
        /// </summary>
        /// <param name="packageId">The ID of the package to create.</param>
        /// <param name="packageVersion">The version of the package to create.</param>
        /// <returns>Empty stream backed by the new package.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="packageId"/> is null or contains only whitespace or <paramref name="packageVersion"/> is null.</exception>
        public override Stream CreatePackage(string packageId, SemanticVersion packageVersion)
        {
            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentNullException("packageId");
            if (packageVersion == null)
                throw new ArgumentNullException("packageVersion");

            InitPackageStore();

            var packagePath = Path.Combine(this.RootPath, packageId);
            Directory.CreateDirectory(packagePath);

            var versionPath = Path.Combine(packagePath, packageId + "." + packageVersion + ".nupkg");

            return new FileStream(versionPath, FileMode.Create, FileAccess.Write, FileShare.Delete);
        }
        /// <summary>
        /// Deletes a package from the store.
        /// </summary>
        /// <param name="packageId">The ID of the package to delete.</param>
        /// <param name="packageVersion">The version of the package to delete.</param>
        /// <exception cref="ArgumentNullException"><paramref name="packageId"/> is null or contains only whitespace or <paramref name="packageVersion"/> is null.</exception>
        public override void DeletePackage(string packageId, SemanticVersion packageVersion)
        {
            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentNullException("packageId");
            if (packageVersion == null)
                throw new ArgumentNullException("packageVersion");

            InitPackageStore();

            var packagePath = Path.Combine(this.RootPath, packageId);

            if (Directory.Exists(packagePath))
            {
                var versionPath = Path.Combine(packagePath, packageId + "." + packageVersion + ".nupkg");
                File.Delete(versionPath);

                if (!Directory.EnumerateFileSystemEntries(packagePath).Any())
                {
                    try { Directory.Delete(packagePath); }
                    catch { }
                }
            }
        }
        /// <summary>
        /// Performs a cleanup and consistency check for the package store.
        /// </summary>
        /// <param name="packageIndex">Interface used to ensure that the package index is correct.</param>
        /// <exception cref="ArgumentNullException"><paramref name="packageIndex"/> is null.</exception>
        public override void Clean(IPackageIndex packageIndex)
        {
            if (packageIndex == null)
                throw new ArgumentNullException("packageIndex");

            InitPackageStore();

            if (Directory.Exists(this.RootPath))
            {
                this.LogDebug("Enumerating directories in {0}...", this.RootPath);

                IEnumerable<string> packageDirectories;
                try
                {
                    packageDirectories = Directory.EnumerateDirectories(this.RootPath);
                }
                catch (Exception ex)
                {
                    this.LogError("Could not access {0}. Skipping feed cleanup. Error: {1}", this.RootPath, ex);
                    return;
                }

                foreach (var packageDirectory in packageDirectories)
                {
                    this.LogDebug("Enumerating files in {0}...", packageDirectory);

                    bool any = false;
                    var packageFileNames = Directory.EnumerateFiles(packageDirectory, "*.nupkg");
                    foreach (var fileName in packageFileNames)
                    {
                        this.LogDebug("Inspecting {0}...", fileName);
                        var localFileName = Path.GetFileName(fileName);

                        any = true;
                        try
                        {
                            using (var stream = this.TryOpenStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
                            {
                                if (stream != null)
                                {
                                    this.LogDebug("Validating {0}...", localFileName);
                                    if (packageIndex.ValidatePackage(stream))
                                    {
                                        this.LogDebug("Verifying that {0} is the correct file name...", localFileName);
                                        stream.Position = 0;
                                        var package = NuGetPackage.ReadFromNupkgFile(stream);
                                        var expectedFileName = package.Id + "." + package.Version + ".nupkg";
                                        if (!string.Equals(localFileName, expectedFileName, StringComparison.OrdinalIgnoreCase))
                                        {
                                            this.LogWarning("File {0} has incorrect name; should be {1}", localFileName, expectedFileName);
                                            this.LogDebug("Renaming {0} to {1}...", localFileName, expectedFileName);

                                            var fullExpectedFileName = Path.Combine(packageDirectory, expectedFileName);
                                            try { File.Delete(fullExpectedFileName); }
                                            catch { }

                                            try
                                            {
                                                File.Move(fileName, fullExpectedFileName);
                                                this.LogDebug("Package renamed.");
                                            }
                                            catch (Exception ex)
                                            {
                                                this.LogError("Could not rename package: " + ex.Message);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            this.LogError("Could not validate {0}: {1}", fileName, ex.Message);
                            StoredProcs.Packages_LogIndexingError(
                                Feed_Id: this.FeedId,
                                PackageFile_Name: localFileName,
                                ErrorMessage_Text: ex.Message,
                                StackTrace_Bytes: Encoding.UTF8.GetBytes(ex.StackTrace ?? string.Empty)
                            ).Execute();
                        }
                    }

                    if (!any)
                    {
                        try
                        {
                            this.LogDebug("Deleting empty directory {0}...", packageDirectory);
                            Directory.Delete(packageDirectory);
                        }
                        catch
                        {
                            this.LogWarning("Directory could not be deleted; it may not be empty.");
                        }
                    }
                }

                packageIndex.RemoveRemaining();
            }
            else
            {
                this.LogDebug("Package root path {0} not found. Nothing to do.", this.RootPath);
            }
        }

        private FileStream TryOpenStream(string fileName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare, int retryCount = 5)
        {
            int currentAttempt = 0;
            Exception lastException = null;
            do
            {
                try
                {
                    return new FileStream(fileName, fileMode, fileAccess, fileShare);
                }
                catch (FileNotFoundException)
                {
                    this.LogDebug("{0} not found.", fileName);
                    return null;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    this.LogWarning("Unable to open file {0}: {1}", fileName, ex);
                    currentAttempt++;
                }

                if (currentAttempt < retryCount)
                {
                    this.LogDebug("Failed to open file; will try again after 5 seconds...");
                    Thread.Sleep(5000);
                }

            } while (currentAttempt < retryCount);

            if (lastException != null)
                throw lastException;

            return null;
        }

        private void LogDebug(string message, params object[] args)
        {
            File.AppendAllText(@"c:\temp\proget-azure-fileshare-extension.log", string.Format(DateTime.Now + "::DEBUG::" + message + "\r\n", args));
        }

        private void LogWarning(string message, params object[] args)
        {
            File.AppendAllText(@"c:\temp\proget-azure-fileshare-extension.log", string.Format(DateTime.Now + "::WARN::" + message + "\r\n", args));
        }

        private void LogError(string message, params object[] args)
        {
            File.AppendAllText(@"c:\temp\proget-azure-fileshare-extension.log", string.Format(DateTime.Now + "::ERROR::" + message + "\r\n", args));
        }
    }
}

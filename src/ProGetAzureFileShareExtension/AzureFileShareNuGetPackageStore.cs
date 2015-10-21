using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Inedo.NuGet;
using Inedo.NuGet.Packages;
using Inedo.ProGet.Data;
using Inedo.ProGet.Extensibility;
using Inedo.ProGet.Extensibility.PackageStores;

namespace ProGetAzureFileShareExtension
{
    /// <summary>
    /// Default implementation of a NuGet package store.
    /// </summary>
    [ProGetComponentProperties("Azure File Storage Nuget Package Store", "Uses an azure file share for the nuget package store")]
    public class AzureFileShareNuGetPackageStore : NuGetPackageStoreBase
    {
        private readonly IFileShareMapper _fileShareMapper;
        private readonly IFileSystemOperations _fileSystemOperations;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFileShareNuGetPackageStore"/> class.
        /// </summary>
        public AzureFileShareNuGetPackageStore()
            : this(new FileShareMapper(), new FileSystemOperations())
        {
        }

        public AzureFileShareNuGetPackageStore(IFileShareMapper fileShareMapper, IFileSystemOperations fileSystemOperations)
        {
            _fileShareMapper = fileShareMapper;
            _fileSystemOperations = fileSystemOperations;
        }

        /// <summary>
        /// The root file system directory of the package store.
        /// </summary>
        [Persistent]
        public string RootPath { get; set; }

        [Persistent]
        public string DriveLetter { get; set; }

        [Persistent]
        public string FileShareName { get; set; }

        [Persistent]
        public string UserName { get; set; }

        [Persistent]
        public string AccessKey { get; set; }

        private void InitPackageStore()
        {
            if (string.IsNullOrWhiteSpace(DriveLetter))
                throw new ArgumentNullException("DriveLetter");
            if (!Regex.IsMatch(DriveLetter, "^[A-Za-z]:$"))
                throw new ArgumentOutOfRangeException("DriveLetter", "DriveLetter must be a single drive letter (A-Z) followed by a colon");

            if (string.IsNullOrWhiteSpace(RootPath))
                throw new ArgumentNullException("RootPath");
            if (!RootPath.ToLower().StartsWith(DriveLetter.ToLower()))
                throw new ArgumentOutOfRangeException("RootPath", "RootPath must be on the drive specified by DriveLetter (ie, if DriveLetter='P:', then RootPath must start with 'P:\'");

            if (string.IsNullOrWhiteSpace(UserName))
                throw new ArgumentNullException("UserName");

            if (string.IsNullOrWhiteSpace(AccessKey))
                throw new ArgumentNullException("AccessKey");

            if (string.IsNullOrWhiteSpace(FileShareName))
                throw new ArgumentNullException("FileShareName");

            var uncPath = string.Format(@"\\{0}.file.core.windows.net\{1}", UserName, FileShareName);

            try
            {
                LogDebug("Mapping network share '{0}' to drive '{1}' with username '{2}'", uncPath, DriveLetter, UserName);
                _fileShareMapper.Mount(DriveLetter, uncPath, UserName, AccessKey);
                LogDebug("Drive mapping successful.");
            }
            catch (Exception ex)
            {
                LogError("Exception occurred mapping drive '{0}' to '{1}' with username '{2}': {3}", DriveLetter, uncPath, UserName, ex);
                throw;
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
            LogDebug("OpenPackage('" + packageId + "', '" + packageVersion + "') called");

            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentNullException("packageId");
            if (packageVersion == null)
                throw new ArgumentNullException("packageVersion");

            InitPackageStore();

            var packagePath = Path.Combine(RootPath, packageId);
            if (!_fileSystemOperations.DirectoryExists(packagePath))
            {
                LogWarning("Attempted to open package '" + packageId + "', version '" + packageVersion + "', in folder '" + packagePath + "' but the folder didn't exist");
                return null;
            }

            var versionPath = Path.Combine(packagePath, packageId + "." + packageVersion + ".nupkg");
            if (!_fileSystemOperations.FileExists(versionPath))
            {
                LogWarning("Attempted to open package '" + packageId + "', version '" + packageVersion + "', at path '" + versionPath + "' but it didn't exist");
                return null;
            }

            try
            {
                return _fileSystemOperations.GetFileStream(versionPath, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
            }
            catch (FileNotFoundException ex)
            {
                LogError("File not found error looking for package '" + packageId + "', version '" + packageVersion + "'." + ex);
                return null;
            }
            catch (DirectoryNotFoundException ex)
            {
                LogError("Directory not found error looking for package '" + packageId + "', version '" + packageVersion + "'." + ex);
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
            LogDebug("CreatePackage('" + packageId + "', '" + packageVersion + "') called");

            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentNullException("packageId");
            if (packageVersion == null)
                throw new ArgumentNullException("packageVersion");

            InitPackageStore();

            try
            {
                var packagePath = Path.Combine(RootPath, packageId);
                LogDebug("Creating package '{0}', version '{1}' in directory '{2}'", packageId, packageVersion, packagePath);
                _fileSystemOperations.CreateDirectory(packagePath);

                var versionPath = Path.Combine(packagePath, packageId + "." + packageVersion + ".nupkg");
                LogDebug("Creating package '{0}', version '{1}' at '{2}'", packageId, packageVersion, versionPath);

                return _fileSystemOperations.GetFileStream(versionPath, FileMode.Create, FileAccess.Write, FileShare.Delete);
            }
            catch (Exception ex)
            {
                LogError("Error creating package '{0}', version '{1}': {2}", packageId, packageVersion, ex);
                throw;
            }
        }

        /// <summary>
        /// Deletes a package from the store.
        /// </summary>
        /// <param name="packageId">The ID of the package to delete.</param>
        /// <param name="packageVersion">The version of the package to delete.</param>
        /// <exception cref="ArgumentNullException"><paramref name="packageId"/> is null or contains only whitespace or <paramref name="packageVersion"/> is null.</exception>
        public override void DeletePackage(string packageId, SemanticVersion packageVersion)
        {
            LogDebug("DeletePackage('" + packageId + "', '" + packageVersion + "') called");

            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentNullException("packageId");
            if (packageVersion == null)
                throw new ArgumentNullException("packageVersion");

            InitPackageStore();

            var packagePath = Path.Combine(RootPath, packageId);

            if (_fileSystemOperations.DirectoryExists(packagePath))
            {
                var versionPath = Path.Combine(packagePath, packageId + "." + packageVersion + ".nupkg");
                LogDebug("Deleting file '" + versionPath + "'.");
                try
                {
                    _fileSystemOperations.DeleteFile(versionPath);
                }
                catch (Exception ex)
                {
                    LogError("Error deleting package '{0}', version '{1}': {2}", packageId, packageVersion, ex);
                    throw;
                }

                if (!_fileSystemOperations.EnumerateFileSystemEntries(packagePath).Any())
                {
                    LogDebug("Deleting folder '" + packagePath + "'.");
                    try
                    {
                        _fileSystemOperations.DeleteDirectory(packagePath);
                    }
                    catch (Exception ex)
                    {
                        LogWarning("Exception while attemptnig to delete folder '{0}': {1}", packagePath, ex);
                    }
                }
            }
            else
            {
                LogWarning("Attempted to delete pacakage ('{0}', '{1}') that didn't exist ", packageId, packageVersion);
            }
        }

        /// <summary>
        /// Performs a cleanup and consistency check for the package store.
        /// </summary>
        /// <param name="packageIndex">Interface used to ensure that the package index is correct.</param>
        /// <exception cref="ArgumentNullException"><paramref name="packageIndex"/> is null.</exception>
        public override void Clean(IPackageIndex packageIndex)
        {
            LogDebug("Clean('" + packageIndex + "') called");

            if (packageIndex == null)
                throw new ArgumentNullException("packageIndex");

            InitPackageStore();

            if (Directory.Exists(RootPath))
            {
                LogDebug("Enumerating directories in {0}...", RootPath);

                IEnumerable<string> packageDirectories;
                try
                {
                    packageDirectories = _fileSystemOperations.EnumerateDirectories(RootPath);
                }
                catch (Exception ex)
                {
                    LogError("Could not access RootPath '{0}'. Skipping feed cleanup. Error: {1}", RootPath, ex);
                    return;
                }

                foreach (var packageDirectory in packageDirectories)
                {
                    LogDebug("Enumerating all nupkg files in packageDirectory '{0}'", packageDirectory);

                    bool any = false;
                    var packageFileNames = _fileSystemOperations.EnumerateFiles(packageDirectory, "*.nupkg");
                    foreach (var fileName in packageFileNames)
                    {
                        LogDebug("Inspecting package '{0}'", fileName);
                        var localFileName = _fileSystemOperations.GetFileName(fileName);

                        any = true;
                        try
                        {
                            using (var stream = TryOpenStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
                            {
                                if (stream != null)
                                {
                                    LogDebug("Validating {0}...", localFileName);
                                    if (packageIndex.ValidatePackage(stream))
                                    {
                                        LogDebug("Verifying that {0} is the correct file name...", localFileName);
                                        stream.Position = 0;
                                        var package = NuGetPackage.ReadFromNupkgFile(stream);
                                        var expectedFileName = package.Id + "." + package.Version + ".nupkg";
                                        if (!string.Equals(localFileName, expectedFileName, StringComparison.OrdinalIgnoreCase))
                                        {
                                            LogWarning("File {0} has incorrect name; should be {1}", localFileName, expectedFileName);
                                            LogDebug("Renaming {0} to {1}...", localFileName, expectedFileName);

                                            var fullExpectedFileName = Path.Combine(packageDirectory, expectedFileName);
                                            if (File.Exists(fullExpectedFileName))
                                            {
                                                try
                                                {
                                                    LogDebug("Deleting target file '" + fullExpectedFileName + "'");
                                                    _fileSystemOperations.DeleteFile(fullExpectedFileName);
                                                }
                                                catch(Exception ex)
                                                {
                                                    LogError("Exception while deleting target file '" + fullExpectedFileName + "': " + ex);
                                                }
                                            }
                                            try
                                            {
                                                LogDebug("Moving package from '{0}' to '{1}'.", fileName, fullExpectedFileName);
                                                _fileSystemOperations.MoveFile(fileName, fullExpectedFileName);
                                                LogDebug("Package renamed.");
                                            }
                                            catch (Exception ex)
                                            {
                                                LogError("Could not rename package: " + ex.Message);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError("Could not validate {0}: {1}", fileName, ex.Message);
                            StoredProcs.Packages_LogIndexingError(FeedId, localFileName, ex.Message, Encoding.UTF8.GetBytes(ex.StackTrace ?? string.Empty)
                            ).Execute();
                        }
                    }

                    if (!any)
                    {
                        try
                        {
                            LogDebug("Deleting empty directory {0}...", packageDirectory);
                            _fileSystemOperations.DeleteDirectory(packageDirectory);
                        }
                        catch
                        {
                            LogWarning("Directory could not be deleted; it may not be empty.");
                        }
                    }
                }

                packageIndex.RemoveRemaining();
            }
            else
            {
                LogDebug("Package root path {0} not found. Nothing to do.", RootPath);
            }
        }

        private FileStream TryOpenStream(string fileName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare, int retryCount = 5)
        {
            var currentAttempt = 0;
            Exception lastException;
            do
            {
                try
                {
                    return (FileStream)_fileSystemOperations.GetFileStream(fileName, fileMode, fileAccess, fileShare);
                }
                catch (FileNotFoundException ex)
                {
                    LogError("{0} not found. {1}", fileName, ex);
                    return null;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    LogWarning("Unable to open file {0}: {1}", fileName, ex);
                    currentAttempt++;
                }

                if (currentAttempt < retryCount)
                {
                    LogDebug("Failed to open file; will try again after 5 seconds...");
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

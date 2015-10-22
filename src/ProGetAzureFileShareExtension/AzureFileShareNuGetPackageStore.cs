using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using log4net;

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
        private ILog _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFileShareNuGetPackageStore"/> class.
        /// </summary>
        public AzureFileShareNuGetPackageStore()
            : this(new FileShareMapper(), new FileSystemOperations())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFileShareNuGetPackageStore"/> class. Used for unit testing
        /// </summary>
        /// <param name="fileShareMapper">An <see cref="IFileShareMapper"/> object to handle mapping of the file share</param>
        /// <param name="fileSystemOperations">An <see cref="IFileSystemOperations"/> object to handle various file system objects</param>
        public AzureFileShareNuGetPackageStore(IFileShareMapper fileShareMapper, IFileSystemOperations fileSystemOperations)
        {
            _fileShareMapper = fileShareMapper;
            _fileSystemOperations = fileSystemOperations;
        }

        /// <summary>
        /// The root file system directory of the package store.
        /// </summary>
        /// <example>P:\ProGetPackages</example>
        [Persistent]
        public string RootPath { get; set; }

        /// <summary>
        /// The drive letter to use to map to the file share
        /// </summary>
        /// <example>P:</example>
        [Persistent]
        public string DriveLetter { get; set; }

        /// <summary>
        /// The name of the share on the remote server
        /// </summary>
        /// <example>If the full unc is \\servername\sharename, it would be 'sharename'</example>
        [Persistent]
        public string FileShareName { get; set; }

        /// <summary>
        /// The username to use to connect to the share. For Azure File Shares, this is the storage account name.
        /// </summary>
        [Persistent]
        public string UserName { get; set; }

        /// <summary>
        /// The password to use to connect to the share. For Azure File Shares, this is either the primary or secondary access key.
        /// </summary>
        [Persistent]
        public string AccessKey { get; set; }

        /// <summary>
        /// The log file to use for logging. If not supplied, logging is disabled.
        /// </summary>
        [Persistent]
        public string LogFileName { get; set; }

        /// <summary>
        /// Initialises the packages store by validating parameters, then connecting to the network share.
        /// </summary>
        /// <exception cref="ArgumentNullException"> if <paramref name="DriveLetter"/> is null or contains only whitespace or <paramref name="RootPath"/> is null or contains only whitespace or <paramref name="UserName"/> is null or contains only whitespace or <paramref name="AccessKey"/> is null or contains only whitespace or <paramref name="FileShareName"/> is null or contains only whitespace.</exception>
        /// <exception cref="ArgumentOutOfRangeException"> if <paramref name="DriveLetter"/> does not match ^[A-Za-z]:$ or <paramref name="RootPath"/> does not start with <paramref name="DriveLetter"/></exception>
        private void InitPackageStore()
        {
            _logger = Logger.Initialise(LogFileName);
            _fileSystemOperations.Logger = _logger;

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
                _logger.DebugFormat("Mapping network share '{0}' to drive '{1}' with username '{2}'", uncPath, DriveLetter, UserName);
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                _fileShareMapper.Mount(DriveLetter, uncPath, UserName, AccessKey);
                stopWatch.Stop();
                _logger.DebugFormat("Drive mapping successful and took {0} milliseconds.", stopWatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("Exception occurred mapping drive '{0}' to '{1}' with username '{2}': {3}", DriveLetter, uncPath, UserName), ex);
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
            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentNullException("packageId");
            if (packageVersion == null)
                throw new ArgumentNullException("packageVersion");

            InitPackageStore();

            _logger.Debug("OpenPackage('" + packageId + "', '" + packageVersion + "') called");

            var packagePath = Path.Combine(RootPath, packageId);
            if (!_fileSystemOperations.DirectoryExists(packagePath))
            {
                _logger.Warn("Attempted to open package '" + packageId + "', version '" + packageVersion + "', in folder '" + packagePath + "' but the folder didn't exist");
                return null;
            }

            var versionPath = Path.Combine(packagePath, packageId + "." + packageVersion + ".nupkg");
            if (!_fileSystemOperations.FileExists(versionPath))
            {
                _logger.Warn("Attempted to open package '" + packageId + "', version '" + packageVersion + "', at path '" + versionPath + "' but it didn't exist");
                return null;
            }

            try
            {
                return _fileSystemOperations.GetFileStream(versionPath, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
            }
            catch (FileNotFoundException ex)
            {
                _logger.Error("File not found error looking for package '" + packageId + "', version '" + packageVersion + "'." + ex);
                return null;
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.Error("Directory not found error looking for package '" + packageId + "', version '" + packageVersion + "'." + ex);
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

            _logger.Debug("CreatePackage('" + packageId + "', '" + packageVersion + "') called");

            try
            {
                var packagePath = Path.Combine(RootPath, packageId);
                _logger.DebugFormat("Creating package '{0}', version '{1}' in directory '{2}'", packageId, packageVersion, packagePath);
                _fileSystemOperations.CreateDirectory(packagePath);

                var versionPath = Path.Combine(packagePath, packageId + "." + packageVersion + ".nupkg");
                _logger.DebugFormat("Creating package '{0}', version '{1}' at '{2}'", packageId, packageVersion, versionPath);

                return _fileSystemOperations.GetFileStream(versionPath, FileMode.Create, FileAccess.Write, FileShare.Delete);
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("Error creating package '{0}', version '{1}': {2}", packageId, packageVersion), ex);
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
            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentNullException("packageId");
            if (packageVersion == null)
                throw new ArgumentNullException("packageVersion");

            InitPackageStore();

            _logger.Debug("DeletePackage('" + packageId + "', '" + packageVersion + "') called");

            var packagePath = Path.Combine(RootPath, packageId);

            if (_fileSystemOperations.DirectoryExists(packagePath))
            {
                var versionPath = Path.Combine(packagePath, packageId + "." + packageVersion + ".nupkg");
                _logger.Debug("Deleting file '" + versionPath + "'.");
                try
                {
                    _fileSystemOperations.DeleteFile(versionPath);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Error deleting package '{0}', version '{1}'", packageId, packageVersion), ex);
                    throw;
                }

                if (!_fileSystemOperations.EnumerateFileSystemEntries(packagePath).Any())
                {
                    _logger.Debug("Deleting folder '" + packagePath + "'.");
                    try
                    {
                        _fileSystemOperations.DeleteDirectory(packagePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(string.Format("Exception while attemptnig to delete folder '{0}')", packagePath), ex);
                    }
                }
            }
            else
            {
                _logger.WarnFormat("Attempted to delete pacakage ('{0}', '{1}') that didn't exist ", packageId, packageVersion);
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

            _logger.Debug("Clean('" + packageIndex + "') called");

            if (Directory.Exists(RootPath))
            {
                _logger.DebugFormat("Enumerating directories in {0}...", RootPath);

                IEnumerable<string> packageDirectories;
                try
                {
                    packageDirectories = _fileSystemOperations.EnumerateDirectories(RootPath);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Could not access RootPath '{0}'. Skipping feed cleanup.", RootPath), ex);
                    return;
                }

                foreach (var packageDirectory in packageDirectories)
                {
                    _logger.DebugFormat("Enumerating all nupkg files in packageDirectory '{0}'", packageDirectory);

                    bool any = false;
                    var packageFileNames = _fileSystemOperations.EnumerateFiles(packageDirectory, "*.nupkg");
                    foreach (var fileName in packageFileNames)
                    {
                        _logger.DebugFormat("Inspecting package '{0}'", fileName);
                        var localFileName = _fileSystemOperations.GetFileName(fileName);

                        any = true;
                        try
                        {
                            using (var stream = TryOpenStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
                            {
                                if (stream != null)
                                {
                                    _logger.DebugFormat("Validating {0}...", localFileName);
                                    if (packageIndex.ValidatePackage(stream))
                                    {
                                        _logger.DebugFormat("Verifying that {0} is the correct file name...", localFileName);
                                        stream.Position = 0;
                                        var package = NuGetPackage.ReadFromNupkgFile(stream);
                                        var expectedFileName = package.Id + "." + package.Version + ".nupkg";
                                        if (!string.Equals(localFileName, expectedFileName, StringComparison.OrdinalIgnoreCase))
                                        {
                                            _logger.WarnFormat("File {0} has incorrect name; should be {1}", localFileName, expectedFileName);
                                            _logger.DebugFormat("Renaming {0} to {1}...", localFileName, expectedFileName);

                                            var fullExpectedFileName = Path.Combine(packageDirectory, expectedFileName);
                                            if (File.Exists(fullExpectedFileName))
                                            {
                                                try
                                                {
                                                    _logger.Debug("Deleting target file '" + fullExpectedFileName + "'");
                                                    _fileSystemOperations.DeleteFile(fullExpectedFileName);
                                                }
                                                catch(Exception ex)
                                                {
                                                    _logger.Error("Exception while deleting target file '" + fullExpectedFileName + "': " + ex);
                                                }
                                            }
                                            try
                                            {
                                                _logger.DebugFormat("Moving package from '{0}' to '{1}'.", fileName, fullExpectedFileName);
                                                _fileSystemOperations.MoveFile(fileName, fullExpectedFileName);
                                                _logger.Debug("Package renamed.");
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.Error("Could not rename package: ", ex);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(string.Format("Could not validate '{0}'", fileName), ex);
                            StoredProcs.Packages_LogIndexingError(FeedId, localFileName, ex.Message, Encoding.UTF8.GetBytes(ex.StackTrace ?? string.Empty)
                            ).Execute();
                        }
                    }

                    if (!any)
                    {
                        try
                        {
                            _logger.DebugFormat("Deleting empty directory {0}...", packageDirectory);
                            _fileSystemOperations.DeleteDirectory(packageDirectory);
                        }
                        catch
                        {
                            _logger.Warn("Directory could not be deleted; it may not be empty.");
                        }
                    }
                }

                packageIndex.RemoveRemaining();
            }
            else
            {
                _logger.DebugFormat("Package root path '{0}' not found. Nothing to do.", RootPath);
            }
        }

        /// <summary>
        /// Attempt to open the filestream, and if fails with any exception other than <see cref="FileNotFoundException" />, retry for <paramref name="retryCount"/> times, with a sleep of 5 seconds between each attempt.
        /// </summary>
        /// <param name="fileName">The full file path to open.</param>
        /// <param name="fileMode"></param>
        /// <param name="fileAccess"></param>
        /// <param name="fileShare"></param>
        /// <param name="retryCount"></param>
        /// <returns>A valid file stream if it can. Null if the file does not exist, or re-throws the last exception if <paramref name="retryCount"/> attempts are exceeded.</returns>
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
                    _logger.Error(string.Format("Filename '{0}' not found.", fileName), ex);
                    return null;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger.Warn(String.Format("Unable to open file '{0}'", fileName), ex);
                    currentAttempt++;
                }

                if (currentAttempt < retryCount)
                {
                    _logger.Debug("Failed to open file; will try again after 5 seconds...");
                    Thread.Sleep(5000);
                }

            } while (currentAttempt < retryCount);

            if (lastException != null)
                throw lastException;
            
            return null;
        }
    }
}

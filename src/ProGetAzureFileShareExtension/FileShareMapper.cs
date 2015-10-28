using System;
using System.IO;
using System.Linq;
using log4net;
using RedDog.Storage.Files;

namespace ProGetAzureFileShareExtension
{
    /// <summary>
    /// An abstraction layer over the <see cref="RedDog.Storage.Files.FilesMappedDrive"/> code to allow for unit testing.
    /// </summary>
    class FileShareMapper : IFileShareMapper
    {
        private static object lockObject = new object();

        public void Mount(string driveLetter, string uncPath, string userName, string accessKey, ILog logger)
        {
            lock(lockObject)
            {
                var mountedShares = FilesMappedDrive.GetMountedShares();

                foreach (var share in mountedShares)
                {
                    if ((share.Path.ToLower() == uncPath.ToLower()) && (share.DriveLetter.ToLower().TrimEnd('\\') == driveLetter.ToLower()))
                    {
                        try
                        {
                            if (Directory.EnumerateDirectories(uncPath).Any() || Directory.EnumerateFiles(uncPath).Any())
                            {
                                logger.DebugFormat("Already connected to '{0}' as drive '{1}'. Skipping re-mount.", share.Path, share.DriveLetter);
                                return;
                            }
                            logger.WarnFormat("Searched for files and directories on the share '{0}', which was mapped as drive '{1}', but didn't find any. " +
                                              "This shouldn't really happen, as there should be something on the share already." +
                                              "Assuming that the share is connected as we were able to query for files successfully; skipping re-mount.", share.Path, share.DriveLetter);
                            return;
                        }
                        catch (Exception ex)
                        {
                            logger.Debug("Ignorable exception while attempting to enumerate files on existing share. Will attempt to remount.", ex);
                        }
                    }
                }

                FilesMappedDrive.Mount(driveLetter, uncPath, userName, accessKey);
            }
        }
    }
}
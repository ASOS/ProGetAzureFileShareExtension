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
        private static readonly object LockObject = new object();

        public void Mount(string driveLetter, string uncPath, string userName, string accessKey, ILog logger)
        {
            if (IsShareConnected(uncPath, logger))
                return;

            lock (LockObject)
            {
                if (IsShareConnected(uncPath, logger))
                    return;

                FilesMappedDrive.Mount(driveLetter, uncPath, userName, accessKey);
            }
        }

        private static bool IsShareConnected(string uncPath, ILog logger)
        {
            try
            {
                if (Directory.EnumerateDirectories(uncPath).Any() || Directory.EnumerateFiles(uncPath).Any())
                {
                    logger.DebugFormat("Already connected to '{0}'. Skipping re-mount.", uncPath);
                    return true;
                }
                logger.WarnFormat(
                    "Searched for files and directories on the share '{0}', but didn't find any. " +
                    "This shouldn't really happen, as there should be something on the share already." +
                    "Assuming that the share is connected as we were able to query for files successfully; skipping re-mount.",
                    uncPath);
                return true;
            }
            catch (Exception ex)
            {
                logger.Debug("Ignorable exception while attempting to enumerate files on existing share. Will attempt to remount.", ex);
                return false;
            }
        }
    }
}
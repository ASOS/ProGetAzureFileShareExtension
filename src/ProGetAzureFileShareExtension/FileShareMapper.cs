using RedDog.Storage.Files;

namespace ProGetAzureFileShareExtension
{
    /// <summary>
    /// An abstraction layer over the <see cref="RedDog.Storage.Files.FilesMappedDrive"/> code to allow for unit testing.
    /// </summary>
    class FileShareMapper : IFileShareMapper
    {
        public void Mount(string driveLetter, string uncPath, string userName, string accessKey)
        {
            FilesMappedDrive.Mount(driveLetter, uncPath, userName, accessKey);
        }
    }
}
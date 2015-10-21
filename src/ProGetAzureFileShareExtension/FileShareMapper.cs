using RedDog.Storage.Files;

namespace ProGetAzureFileShareExtension
{
    class FileShareMapper : IFileShareMapper
    {
        public void Mount(string driveLetter, string uncPath, string userName, string accessKey)
        {
            FilesMappedDrive.Mount(driveLetter, uncPath, userName, accessKey);
        }
    }
}
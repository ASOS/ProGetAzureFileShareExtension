namespace ProGetAzureFileShareExtension
{
    public interface IFileShareMapper
    {
        void Mount(string driveLetter, string uncPath, string userName, string accessKey);
    }
}
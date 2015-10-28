using log4net;

namespace ProGetAzureFileShareExtension
{
    /// <summary>
    /// An abstraction layer to allow for unit testing.
    /// </summary>
    public interface IFileShareMapper
    {
        void Mount(string driveLetter, string uncPath, string userName, string accessKey, ILog logger);
    }
}

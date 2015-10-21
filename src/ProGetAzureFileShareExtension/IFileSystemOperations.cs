using System.Collections.Generic;
using System.IO;

namespace ProGetAzureFileShareExtension
{
    /// <summary>
    /// Interface to allow for unit testing
    /// </summary>
    public interface IFileSystemOperations
    {
        bool DirectoryExists(string path);
        bool FileExists(string path);
        Stream GetFileStream(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare);
        void CreateDirectory(string path);
        void DeleteDirectory(string path);
        void MoveFile(string sourceFileName, string destFileName);
        void DeleteFile(string path);
        IEnumerable<string> EnumerateDirectories(string path);
        IEnumerable<string> EnumerateFiles(string path, string searchPattern);
        string GetFileName(string path);
        IEnumerable<string> EnumerateFileSystemEntries(string path);
    }
}
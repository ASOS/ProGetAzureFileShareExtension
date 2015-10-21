using System.Collections.Generic;
using System.IO;

namespace ProGetAzureFileShareExtension
{
    class FileSystemOperations : IFileSystemOperations
    {
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public Stream GetFileStream(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            return new FileStream(path, fileMode, fileAccess, fileShare);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public void DeleteDirectory(string path)
        {
            Directory.Delete(path);
        }

        public void MoveFile(string sourceFileName, string destFileName)
        {
            File.Move(sourceFileName, destFileName);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public IEnumerable<string> EnumerateDirectories(string path)
        {
            return Directory.EnumerateDirectories(path);
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            return Directory.EnumerateFiles(path, searchPattern);
        }

        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public IEnumerable<string> EnumerateFileSystemEntries(string path)
        {
            return Directory.EnumerateFileSystemEntries(path);
        }
    }
}
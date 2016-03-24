/*
Copyright 2016 ASOS.com Limited

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System.Collections.Generic;
using System.IO;
using log4net;

namespace ProGetAzureFileShareExtension
{
    /// <summary>
    /// A thin abstraction layer over various file system operations to allow for unit testing.
    /// </summary>
    class FileSystemOperations : IFileSystemOperations
    {
        private ILog _logger;

        public bool DirectoryExists(string path)
        {
            using (new StopWatchLogger(_logger, string.Format("Directory.Exists('{0}')", path)))
            {
                return Directory.Exists(path);
            }
        }

        public bool FileExists(string path)
        {
            using (new StopWatchLogger(_logger, string.Format("File.Exists('{0}')", path)))
            {
                return File.Exists(path);
            }
        }

        public Stream GetFileStream(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            using (new StopWatchLogger(_logger, string.Format("GetFileStream('{0}', '{1}', '{2}', '{3}')", path, fileMode, fileAccess, fileShare)))
            {
                return new FileStream(path, fileMode, fileAccess, fileShare);
            }
        }

        public void CreateDirectory(string path)
        {
            using (new StopWatchLogger(_logger, string.Format("Directory.CreateDirectory('{0}')", path)))
            {
                Directory.CreateDirectory(path);
            }
        }

        public void DeleteDirectory(string path)
        {
            using (new StopWatchLogger(_logger, string.Format("Directory.Delete('{0}')", path)))
            {
                Directory.Delete(path);
            }
        }

        public void MoveFile(string sourceFileName, string destFileName)
        {
            using (new StopWatchLogger(_logger, string.Format("File.Move('{0}', '{1}')", sourceFileName, destFileName)))
            {
                File.Move(sourceFileName, destFileName);
            }
        }

        public void DeleteFile(string path)
        {
            using (new StopWatchLogger(_logger, string.Format("File.Delete('{0}')", path)))
            {
                File.Delete(path);
            }
        }

        public IEnumerable<string> EnumerateDirectories(string path)
        {
            using (new StopWatchLogger(_logger, string.Format("Directory.EnumerateDirectories('{0}')", path)))
            {
                return Directory.EnumerateDirectories(path);
            }
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            using (new StopWatchLogger(_logger, string.Format("Directory.EnumerateFiles('{0}', '{1}')", path, searchPattern)))
            {
                return Directory.EnumerateFiles(path, searchPattern);
            }
        }

        public string GetFileName(string path)
        {
            using (new StopWatchLogger(_logger, string.Format("Path.GetFileName('{0}')", path)))
            {
                return Path.GetFileName(path);
            }        
        }

        public IEnumerable<string> EnumerateFileSystemEntries(string path)
        {
            using (new StopWatchLogger(_logger, string.Format("Directory.EnumerateFileSystemEntries('{0}')", path)))
            {
                return Directory.EnumerateFileSystemEntries(path);
            }
        }

        public ILog Logger {
            set { _logger = value; }
        }
    }
}
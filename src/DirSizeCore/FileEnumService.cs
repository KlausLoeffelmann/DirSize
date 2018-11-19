using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Text;

namespace DirSize
{
    internal class FileEnumService : IIoEnumService
    {
        public DriveInfo[] GetDrives() => DriveInfo.GetDrives();

        public IEnumerable<SimpleIoItemInfoStructure> GetFilesAndDirs(string drive)
        {
            var options = new EnumerationOptions() { RecurseSubdirectories = false };
            return new FileSystemEnumerable<SimpleIoItemInfoStructure>(drive,
                                                   (ref FileSystemEntry entry) => new SimpleIoItemInfoStructure
                                                   {
                                                       IsFolder = entry.IsDirectory,
                                                       Name = entry.FileName.ToString(),
                                                       Path = entry.ToFullPath(),
                                                       Size = entry.Length
                                                   },
                                                   options)
            {
                ShouldIncludePredicate = (ref FileSystemEntry entry) => true
            };
        }

        public IEnumerable<string> GetFilesAndDirsRecursive(string drive)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetFolderSize(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetFolderSizeRecursive(string path)
        {
            throw new NotImplementedException();
        }
    }
}

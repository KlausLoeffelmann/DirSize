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
                                                       Size = entry.Length,
                                                       DateModified = entry.LastWriteTimeUtc,
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

        public long GetFolderSize(string path)
        {
            throw new NotImplementedException();
        }

        public long GetFolderSizeRecursive(string path, Action<long> sumBuilderProgressCallBack)
        {
            long currentSum = 0;
            long currentItem = 0;
            var options = new EnumerationOptions() { RecurseSubdirectories = false };
            return (new FileSystemEnumerable<long>(
                    path,
                    (ref FileSystemEntry entry) => {
                        currentSum += entry.Length;
                        if (currentItem++==100)
                        {
                            currentItem = 0;
                            sumBuilderProgressCallBack.Invoke(currentSum);
                        }
                        return entry.Length;
                    },
                    new EnumerationOptions() { RecurseSubdirectories = true })
            {
                ShouldIncludePredicate = (ref FileSystemEntry entry) => !entry.IsDirectory
            }).Sum();
        }
    }
}

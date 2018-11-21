using ByteSizeLib;
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

        public ByteSize GetFolderSize(string path)
        {
            throw new NotImplementedException();
        }

        public (long totalElements, ByteSize totalBytesUsed) 
            GetFolderSizeRecursive(string path, 
                                   Action<(long additionalElementsCounted,ByteSize additionalBytesUsed)> sumBuilderProgressCallBack)
        {
            long additionalBytes = 0;
            long itemCount = 0;
            var options = new EnumerationOptions() { RecurseSubdirectories = false };

            var totalBytesUsed = (new FileSystemEnumerable<long>(
                                    path,
                                    (ref FileSystemEntry entry) =>
                                    {
                                        additionalBytes += entry.Length;
                                        if (itemCount++ % 100 == 0)
                                        {
                                            sumBuilderProgressCallBack.Invoke((100, new ByteSize(additionalBytes)));
                                            additionalBytes = 0;
                                        }
                                        return entry.Length;
                                    },
                                    new EnumerationOptions() { RecurseSubdirectories = true })
                                            {
                                                ShouldIncludePredicate = (ref FileSystemEntry entry) => !entry.IsDirectory
                                            }).Sum();

            return (itemCount, new ByteSize(totalBytesUsed));
        }
    }
}

﻿using ByteSizeLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DirSize
{
    public struct SimpleIoItemInfoStructure
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public long? Size { get; set; }
        public long? Count { get; set; }
        public bool IsFolder { get; set; }
        public DateTimeOffset DateModified { get; set; }
    }

    public interface IIoEnumService
    {
        DriveInfo[] GetDrives();

        IEnumerable<SimpleIoItemInfoStructure> GetFilesAndDirs(string drive);
        IEnumerable<string> GetFilesAndDirsRecursive(string drive);
        ByteSize GetFolderSize(string path);

        (long totalElements, ByteSize totalBytesUsed) GetFolderSizeRecursive(
            string path,
            Action<(long additionalElementsCounted, ByteSize additionalBytesUsed)> sumBuilderProgressCallBack);
    }
}

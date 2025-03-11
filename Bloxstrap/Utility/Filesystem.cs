﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Bloxstrap.Utility
{
    internal static class Filesystem
    {
        internal static long GetFreeDiskSpace(string path)
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                // https://github.com/bloxstraplabs/bloxstrap/issues/1648#issuecomment-2192571030
                if (path.ToUpperInvariant().StartsWith(drive.Name))
                    return drive.AvailableFreeSpace;
            }

            return -1;
        }

        internal static void AssertReadOnly(string filePath)
        {
            var fileInfo = new FileInfo(filePath);

            if (!fileInfo.Exists || !fileInfo.IsReadOnly)
                return;

            fileInfo.IsReadOnly = false;
            App.Logger.WriteLine("Filesystem::AssertReadOnly", $"The following file was set as read-only: {filePath}");
        }

        internal static void AssertReadOnlyDirectory(string directoryPath)
        {
            var directory = new DirectoryInfo(directoryPath) { Attributes = FileAttributes.Normal };

            foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
                info.Attributes = FileAttributes.Normal;

            App.Logger.WriteLine("Filesystem::AssertReadOnlyDirectory", $"The following directory was set as read-only: {directoryPath}");
        }
    }
}

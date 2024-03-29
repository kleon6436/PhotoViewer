﻿using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Kchary.PhotoViewer.Helpers
{
    internal static class SafeNativeMethods
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);
    }

    public sealed class NaturalDirectoryInfoNameComparer : IComparer<DirectoryInfo>
    {
        public int Compare(DirectoryInfo x, DirectoryInfo y)
        {
            return SafeNativeMethods.StrCmpLogicalW(x?.Name, y?.Name);
        }
    }

    public sealed class NaturalFileInfoNameComparer : IComparer<FileInfo>
    {
        public int Compare(FileInfo x, FileInfo y)
        {
            return SafeNativeMethods.StrCmpLogicalW(x?.Name, y?.Name);
        }
    }
}
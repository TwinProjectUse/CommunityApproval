using System;
using System.IO;

namespace IOApproval
{
    public enum SuffixStyle { Windows, Binary, Metric }

    internal static class Helpers
    {
       
        private static readonly string[] SizeWindowsSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

       
        private static readonly string[] SizeBinarySuffixes = { "bytes", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB" };

        
        private static readonly string[] SizeMetricSuffixes = { "bytes", "kB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        internal static string ToSizeWithSuffix(long value, SuffixStyle style, int decimalPlaces = 1)
        {
            var newBase = 1024;
            if (style == SuffixStyle.Metric)
            {
                newBase = 1000;
            }

            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            
            int mag = (int)Math.Log(value, newBase);

            
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            if (style == SuffixStyle.Metric)
            {
                adjustedSize = value / (decimal)(Math.Pow(newBase, mag));
            }

            
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= newBase;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                                adjustedSize,
                                GetSuffixAtIndex(style, mag));
        }

        private static string GetSuffixAtIndex(SuffixStyle style, int index)
        {
            switch (style)
            {
                case SuffixStyle.Binary:
                    return SizeBinarySuffixes[index];
                case SuffixStyle.Metric:
                    return SizeMetricSuffixes[index];
                case SuffixStyle.Windows:
                    return SizeWindowsSuffixes[index];
            }
            return string.Empty;
        }

        
        internal static bool? IsDirFile(this string path)
        {
            bool? result = null; if (Directory.Exists(path) || File.Exists(path))
            {
               
                var fileAttr = File.GetAttributes(path);
                if (fileAttr.HasFlag(FileAttributes.Directory))
                    result = true;
                else result = false;
            }
            return result;
        }

        
        internal static string CorrectFileDestinationPath(string source, string destination)
        {
            var destinationFile = destination;
            if (destination.IsDirFile() == true)
            {
                destinationFile = Path.Combine(destination, Path.GetFileName(source));
            }
            return destinationFile;
        }


        internal static DirectorySizeInfo DirSize(DirectoryInfo d)
        {
            DirectorySizeInfo size = new DirectorySizeInfo();

            try
            {
                
                var fis = d.GetFiles();
                foreach (var fi in fis)
                {
                    size.Size += fi.Length;
                }
                size.FileCount += fis.Length;

               
                var dis = d.GetDirectories();
                size.DirectoryCount += dis.Length;
                foreach (var di in dis)
                {
                    size += DirSize(di);
                }
            }
            catch
            {
            }

            return size;
        }

        internal sealed class DirectorySizeInfo
        {
            public long FileCount = 0;
            public long DirectoryCount = 0;
            public long Size = 0;

            public static DirectorySizeInfo operator +(DirectorySizeInfo s1, DirectorySizeInfo s2)
            {
                return new DirectorySizeInfo()
                {
                    DirectoryCount = s1.DirectoryCount + s2.DirectoryCount,
                    FileCount = s1.FileCount + s2.FileCount,
                    Size = s1.Size + s2.Size
                };
            }
        }
    }
}

namespace TWLauncher.Utils {
    /// <summary>
    /// 字节数格式化工具，将字节数转为可读字符串（B / KB / MB / GB）。
    /// </summary>
    internal static class ByteUtil {
        /// <summary>
        /// 将字节数格式化为可读字符串，如 "1.50 GB"、"128.0 MB"。
        /// </summary>
        public static string Format(long bytes) {
            const double kilobyte = 1024.0;
            const double megabyte = kilobyte * 1024.0;
            const double gigabyte = megabyte * 1024.0;

            if (bytes >= gigabyte)
                return (bytes / gigabyte).ToString("0.00") + " GB";
            if (bytes >= megabyte)
                return (bytes / megabyte).ToString("0.0") + " MB";
            if (bytes >= kilobyte)
                return (bytes / kilobyte).ToString("0.0") + " KB";
            return bytes + " B";
        }
    }
}

using System;
using System.IO;

namespace TWLauncher.Utils {
    /// <summary>
    /// 简易日志工具：每次启动生成新文件，线程安全。
    /// </summary>
    internal static class LogUtil {
        private static readonly object _lock = new object();
        private static readonly string LogPath;

        static LogUtil() {
            try {
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                Directory.CreateDirectory(dir);
                LogPath = Path.Combine(dir, string.Format("launcher_{0:yyyy-MM-dd_HH-mm-ss}.log", DateTime.Now));
            } catch {
                LogPath = Path.Combine(Path.GetTempPath(), string.Format("TWLauncher_{0:yyyy-MM-dd_HH-mm-ss}.log", DateTime.Now));
            }
        }

        // 写入一行带时间戳的日志
        public static void Info(string msg) {
            string line = string.Format("[{0:HH:mm:ss.fff}] {1}", DateTime.Now, msg);
            System.Diagnostics.Debug.WriteLine(line);
            lock (_lock) {
                File.AppendAllText(LogPath, line + Environment.NewLine);
            }
        }

        // 写入错误日志
        public static void Error(string msg) {
            Info("[ERROR] " + msg);
        }

        // 写入异常日志
        public static void Error(Exception ex) {
            Error(ex.ToString());
        }
    }
}

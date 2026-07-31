using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using TWLauncher.Models;

namespace TWLauncher.Utils {
    /// <summary>
    /// Java 工具：验证 javaw.exe 版本、弹窗选择 Java。
    /// </summary>
    internal static class JavaUtil {

        /// <summary>弹窗选择 javaw.exe 并验证，有效返回 JavaPath，取消或无效返回 null。</summary>
        public static JavaPath PickJava() {
            while (true) {
                var dialog = new Microsoft.Win32.OpenFileDialog {
                    Title = "请选择 javaw.exe",
                    Filter = "javaw.exe|javaw.exe",
                    FileName = "javaw.exe"
                };
                if (dialog.ShowDialog() != true) return null;

                var validated = TryValidateJava(Path.GetDirectoryName(dialog.FileName));
                if (validated != null) return validated;

                var retry = MessageBox.Show("选择的 Java 版本低于 17 或无法识别，是否重新选择？", "Java 无效",
                    MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (retry == MessageBoxResult.No) return null;
            }
        }

        /// <summary>验证指定 bin 目录中的 javaw.exe 是否为 Java 17+，有效则返回 JavaPath，否则返回 null。</summary>
        public static JavaPath TryValidateJava(string dir) {
            try { dir = Path.GetFullPath(dir); } catch { return null; }
            if (!dir.EndsWith("\\")) dir += "\\";

            string javawPath = dir + "javaw.exe";
            if (!File.Exists(javawPath)) return null;

            int major = 0; string raw = null;
            try {
                var psi = new ProcessStartInfo {
                    FileName = javawPath,
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using (var proc = Process.Start(psi)) {
                    string output = proc.StandardError.ReadToEnd() + proc.StandardOutput.ReadToEnd();
                    if (!proc.WaitForExit(5000)) { proc.Kill(); return null; }
                    if (proc.ExitCode != 0) return null;

                    var match = Regex.Match(output, @"version ""?([^""\s]+)""?");
                    if (!match.Success) return null;

                    raw = match.Groups[1].Value;
                    if (raw.StartsWith("1."))
                        int.TryParse(raw.Substring(2, raw.IndexOf('.', 2) - 2), out major);
                    else
                        int.TryParse(raw.Substring(0, raw.IndexOf('.')), out major);

                    if (major < 17) return null;
                }
            } catch { return null; }

            return new JavaPath(dir, raw);
        }
    }
}

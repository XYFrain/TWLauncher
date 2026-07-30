using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TWLauncher.Utils;

namespace TWLauncher.Service {
    /// <summary>
    /// Java 检测服务：扫描系统中所有 Java 目录。
    /// </summary>
    internal static class JavaService {

        // ===================== 公开方法 =====================

        /// <summary>
        /// 搜索 PATH → JAVA_HOME → 全盘 Java 目录 → 已保存配置。
        /// 返回 [{path, version}]，仅含 Java 17+。
        /// </summary>
        public static async Task<List<Dictionary<string, string>>> CheckAsync() {
            var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var javaList = new List<Dictionary<string, string>>();
            var tasks = new List<Task>();

            // 1. PATH
            tasks.Add(Task.Run(() => { ScanPath(found, javaList); }));

            // 2. JAVA_HOME
            tasks.Add(Task.Run(() => { ScanJavaHome(found, javaList); }));

            // 3. 全盘扫描 Java 目录
            tasks.Add(Task.Run(() => {
                foreach (var drive in DriveInfo.GetDrives()) {
                    try {
                        ScanJavaDir(Path.Combine(drive.Name, "Java"), found, javaList);
                        ScanJavaDir(Path.Combine(drive.Name, "Program Files", "Java"), found, javaList);
                    } catch { }
                }
            }));

            // 4. 已保存配置
            string configDir = Controller.ConfigController.JavaPath;
            if (!string.IsNullOrEmpty(configDir))
                TryAdd(configDir, found, javaList);

            // 同步运行扫描
            await Task.WhenAll(tasks);

            LogUtil.Info(string.Format("[Java] 找到 {0} 个:", javaList.Count));
            foreach (var e in javaList)
                LogUtil.Info(string.Format("[Java]   Java {0}: {1}", e["version"], e["path"]));

            return javaList;
        }

        // ===================== 多路检测java =====================

        // 检测PATH目录
        private static void ScanPath(HashSet<string> found, List<Dictionary<string, string>> javaList) {
            string pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
            foreach (string dir in pathEnv.Split(Path.PathSeparator)) {
                string javaPath = dir.Trim().Trim('"');
                if (string.IsNullOrEmpty(javaPath)) continue;
                TryAdd(javaPath, found, javaList);
            }
        }
        // 检测JAVA_HOME
        private static void ScanJavaHome(HashSet<string> found, List<Dictionary<string, string>> javaList) {
            string javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (string.IsNullOrEmpty(javaHome)) return;
            TryAdd(Path.Combine(javaHome.Trim(), "bin"), found, javaList);
        }

        // 扫描指定位置
        private static void ScanJavaDir(string rootPath, HashSet<string> found, List<Dictionary<string, string>> javaList) {
            try {
                if (!Directory.Exists(rootPath)) return;
                TryAdd(rootPath, found, javaList);
                foreach (string subDir in Directory.GetDirectories(rootPath))
                    ScanJavaDir(subDir, found, javaList);
            } catch { }
        }

        // ===================== 内部方法 =====================

        // 检测javaw能否正常运行 & 版本 ≥ 17
        private static void TryAdd(string dir, HashSet<string> found, List<Dictionary<string, string>> javaList) {
            // 规范化 → 去重
            try { dir = Path.GetFullPath(dir); } catch { }
            if (!dir.EndsWith("\\")) dir += "\\";
            if (!found.Add(dir)) return;

            string javawPath = dir + "javaw.exe";
            if (!File.Exists(javawPath)) return;

            // 执行 javaw -version，解析主版本号，过滤 < 17
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
                    if (!proc.WaitForExit(5000)) { proc.Kill(); return; }
                    if (proc.ExitCode != 0) return;

                    var match = Regex.Match(output, @"version ""?([^""\s]+)""?");
                    if (!match.Success) return;

                    raw = match.Groups[1].Value;
                    if (raw.StartsWith("1."))
                        int.TryParse(raw.Substring(2, raw.IndexOf('.', 2) - 2), out major);
                    else
                        int.TryParse(raw.Substring(0, raw.IndexOf('.')), out major);

                    if (major < 17) return;
                }
            } catch { return; }

            javaList.Add(new Dictionary<string, string> {
                ["path"] = dir,
                ["version"] = raw
            });
        }
    }
}

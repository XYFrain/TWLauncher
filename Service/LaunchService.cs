using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TWLauncher.Controller;
using TWLauncher.Utils;

namespace TWLauncher.Service {
    /// <summary>
    /// 启动服务：拼装 JVM 参数并启动 Minecraft 进程。
    /// </summary>
    internal class LaunchService {
        public static void Launch() {
            string javaPath = Path.Combine(ConfigController.JavaPath, "java.exe");
            if (!File.Exists(javaPath))
                throw new Exception(string.Format("找不到Java，请检查路径: {0}", javaPath));

            string mainClass;
            if (!JsonUtil.TryGetString(JsonUtil.ReadJson(Paths.MinecraftJson), "mainClass", out mainClass) || string.IsNullOrEmpty(mainClass))
                throw new Exception("Minecraft.json中缺少mainClass");

            string maxMemory = string.Format("-Xmx{0}M", ConfigController.MaxMemory);
            string jvmArgs = string.Join(" ", BuildJvmArgs());
            string gameArgs = string.Join(" ", BuildGameArgs());
            string classpath = BuildClasspath();

            string arguments = string.Format("{0} {1} -cp \"{2}\" {3} {4}", maxMemory, jvmArgs, classpath, mainClass, gameArgs);

            LogUtil.Info(string.Format("[启动] java: {0}", javaPath));
            LogUtil.Info(string.Format("[启动] args: {0}", arguments));

            ProcessStartInfo startInfo = new ProcessStartInfo {
                FileName = Path.Combine(Path.GetDirectoryName(javaPath), "javaw.exe"),
                Arguments = arguments,
                WorkingDirectory = Paths.GameRoot,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(startInfo)?.Dispose();
        }
        private static List<string> BuildJvmArgs() {
            var jvmParts = new List<string>();
            // jvm 参数
            object[] jvmArgs;
            if (JsonUtil.TryGetDict(JsonUtil.ReadJson(Paths.MinecraftJson), "arguments", out var arguments)
                && JsonUtil.TryGetArray(arguments, "jvm", out jvmArgs)) {
                foreach (object item in jvmArgs) {
                    if (item is string str) {
                        if (str == "-cp" || str.StartsWith("-cp ") || str == "${classpath}") continue;
                        jvmParts.Add(str.Replace(" ", ""));
                    } else if (item is Dictionary<string, object> obj) {
                        if (!RuleUtil.IsAllowedOnWindows(obj)) continue;
                        if (JsonUtil.TryGetString(obj, "value", out string s))
                            jvmParts.Add(s.Replace(" ", ""));
                    }
                }
            }
            // 替换占位符
            for (int i = 0; i < jvmParts.Count; i++) {
                jvmParts[i] = jvmParts[i]
                    .Replace("${natives_directory}", Paths.NativesPath)
                    .Replace("${launcher_name}", "TWLauncher")
                    .Replace("${launcher_version}", "1.0.0");
            }

            foreach (string i in jvmParts) {
                LogUtil.Info(string.Format("[启动] JvmArgs: {0}", i));
            }

            return jvmParts;
        }
        private static List<string> BuildGameArgs() {
            var gameParts = new List<string>();

            // 解析 arguments.game（只处理裸字符串，跳过对象）
            object[] gameArgs;
            if (JsonUtil.TryGetDict(JsonUtil.ReadJson(Paths.MinecraftJson), "arguments", out var arguments)
                && JsonUtil.TryGetArray(arguments, "game", out gameArgs)) {
                for (int i = 0; i < gameArgs.Length; i++) {
                    if (gameArgs[i] is string str) {
                        // 跳过正版登录相关参数及其值
                        if (str == "--accessToken" || str == "--clientId" || str == "--xuid" || str == "--userType") {
                            i++; continue;
                        }
                        gameParts.Add(str);
                    }
                }
            }
            // 读取版本 ID
            string version;
            JsonUtil.TryGetString(JsonUtil.ReadJson(Paths.MinecraftJson), "id", out version);
            // 读取玩家 UUID
            string playerName = ConfigController.PlayerName;
            if (string.IsNullOrEmpty(playerName)) playerName = "User";
            string uuid;
            byte[] data = Encoding.UTF8.GetBytes("OfflinePlayer:" + playerName);
            using (MD5 md5 = MD5.Create()) {
                byte[] hash = md5.ComputeHash(data);
                hash[6] = (byte)((hash[6] & 0x0f) | 0x30);
                hash[8] = (byte)((hash[8] & 0x3f) | 0x80);
                string hex = BitConverter.ToString(hash).Replace("-", "").ToLower();
                uuid = string.Format("{0}-{1}-{2}-{3}-{4}",
                    hex.Substring(0, 8), hex.Substring(8, 4),
                    hex.Substring(12, 4), hex.Substring(16, 4), hex.Substring(20, 12));
            }
            // 替换占位符
            for (int i = 0; i < gameParts.Count; i++) {
                gameParts[i] = gameParts[i]
                    .Replace("${auth_player_name}", playerName)
                    .Replace("${version_name}", version)
                    .Replace("${game_directory}", Paths.GameRoot)
                    .Replace("${assets_root}", Paths.AssetsPath)
                    .Replace("${assets_index_name}", "5")
                    .Replace("${auth_uuid}", uuid)
                    .Replace("${version_type}", "release");
            }

            foreach (string i in gameParts) {
                LogUtil.Info(string.Format("[启动] GameArgs: {0}", i));
            }

            return gameParts;
        }
        private static string BuildClasspath() {
            var classpath = new List<string>();

            if (File.Exists(Paths.Minecraftjar))
                classpath.Add(Paths.Minecraftjar);

            Dictionary<string, object> json = JsonUtil.ReadJson(Paths.MinecraftJson);
            object[] libraries;
            if (JsonUtil.TryGetArray(json, "libraries", out libraries)) {
                foreach (object libValue in libraries) {
                    Dictionary<string, object> library = libValue as Dictionary<string, object>;
                    if (library == null || !RuleUtil.IsAllowedOnWindows(library))
                        continue;

                    // 获取 library 路径
                    string libPath = null;
                    Dictionary<string, object> downloads;
                    if (JsonUtil.TryGetDict(library, "downloads", out downloads)) {
                        Dictionary<string, object> artifact;
                        if (JsonUtil.TryGetDict(downloads, "artifact", out artifact))
                            JsonUtil.TryGetString(artifact, "path", out libPath);
                    }
                    if (string.IsNullOrEmpty(libPath)) {
                        string name;
                        if (JsonUtil.TryGetString(library, "name", out name)) {
                            string[] parts = name.Split(':');
                            if (parts.Length >= 3) {
                                libPath = string.Format("{0}/{1}/{2}/{1}-{2}.jar",
                                    parts[0].Replace('.', '/'), parts[1], parts[2]);
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(libPath))
                        continue;

                    string fullPath = Path.Combine(Paths.LibrariesPath, libPath);
                    if (File.Exists(fullPath))
                        classpath.Add(fullPath);
                }
            }

            foreach (string i in classpath) {
                LogUtil.Info(string.Format("[启动] ClassPath: {0}", i));
            }

            return string.Join(";", classpath);
        }
    }
}

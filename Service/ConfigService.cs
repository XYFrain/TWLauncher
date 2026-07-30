using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using TWLauncher.Utils;

namespace TWLauncher.Service {
    /// <summary>
    /// 配置读写服务：磁盘 JSON ↔ 内存字典。外部不直接调，由 ConfigController 包装。
    /// </summary>
    internal static class ConfigService {

        private static readonly string ConfigPath = Paths.LauncherConfigPath;

        private static Dictionary<string, object> _config;

        public static string JavaPath {
            get { if (_config == null) Load(); return JsonUtil.TryGetString(_config, "javaPath", out var s) ? s : ""; }
            set { if (_config == null) Load(); _config["javaPath"] = value; Save(); }
        }

        /// <summary>玩家名（离线模式）。</summary>
        public static string PlayerName {
            get { if (_config == null) Load(); return JsonUtil.TryGetString(_config, "playerName", out var s) ? s : "User"; }
            set { if (_config == null) Load(); _config["playerName"] = value; Save(); }
        }

        public static int MaxMemory {
            get { if (_config == null) Load(); return JsonUtil.TryGetInt64(_config, "maxMemory", out var v) ? (int)v : 0; }
            set { if (_config == null) Load(); _config["maxMemory"] = value; Save(); }
        }

        public static List<Dictionary<string, string>> JavaList {
            get {
                if (_config == null) Load();
                var result = new List<Dictionary<string, string>>();
                if (_config.TryGetValue("javaPathList", out var obj) && obj is object[] arr) {
                    foreach (var item in arr) {
                        if (item is Dictionary<string, object> dict) {
                            result.Add(new Dictionary<string, string> {
                                ["path"] = dict.TryGetValue("path", out var p) ? p?.ToString() ?? "" : "",
                                ["version"] = dict.TryGetValue("version", out var v) ? v?.ToString() ?? "" : ""
                            });
                        }
                    }
                }
                return result;
            }
            set {
                if (_config == null) Load();
                var arr = new object[value.Count];
                for (int i = 0; i < value.Count; i++) {
                    var item = new Dictionary<string, object> { ["path"] = value[i]["path"] };
                    if (value[i].ContainsKey("version"))
                        item["version"] = value[i]["version"];
                    arr[i] = item;
                }
                _config["javaPathList"] = arr;
                Save();
            }
        }

        public static List<(string path, string displayName)> GetJavaEntries() {
            var result = new List<(string path, string displayName)>();
            foreach (var entry in JavaList) {
                string path = entry["path"];
                string version = entry.ContainsKey("version") ? entry["version"] : "";
                result.Add((path, string.Format("Java {0}: {1}", version, path)));
            }
            return result;
        }

        // 磁盘 → 内存
        public static void Load() {
            if (File.Exists(ConfigPath))
                _config = JsonUtil.ReadJson(ConfigPath);
            else
                Save();
        }

        // 内存 → 磁盘
        public static void Save() {
            if (_config == null)
                _config = new Dictionary<string, object> {
                    { "playerName", "User" },
                    { "maxMemory", 4096 },
                    { "javaPath", "H:\\Java\\jdk-1.8\\bin\\" }
                };

            string json = new JavaScriptSerializer().Serialize(_config);
            File.WriteAllText(ConfigPath, json);
        }
    }
}

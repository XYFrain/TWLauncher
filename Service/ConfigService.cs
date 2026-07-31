using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Web.Script.Serialization;
using TWLauncher.Models;
using TWLauncher.Utils;

namespace TWLauncher.Service {
    /// <summary>
    /// 配置读写服务：磁盘 JSON ↔ 内存字典。外部不直接调，由 ConfigController 包装。
    /// </summary>
    internal static class ConfigService {

        private static readonly string ConfigPath = Paths.LauncherConfigPath;
        private static Dictionary<string, object> _config;
        private static ObservableCollection<JavaPath> javaPathList = new ObservableCollection<JavaPath>();
        private static bool _javaPathListEventRegistered;

        public static string JavaPath {
            get { if (_config == null) Load(); return JsonUtil.TryGetString(_config, "javaPath", out var s) ? s : ""; }
            set { if (_config == null) Load(); _config["javaPath"] = value; Save(); }
        }

        public static string PlayerName {
            get { if (_config == null) Load(); return JsonUtil.TryGetString(_config, "playerName", out var s) ? s : "User"; }
            set { if (_config == null) Load(); _config["playerName"] = value; Save(); }
        }

        public static int MaxMemory {
            get { if (_config == null) Load(); return JsonUtil.TryGetInt64(_config, "maxMemory", out var v) ? (int)v : 0; }
            set { if (_config == null) Load(); _config["maxMemory"] = value; Save(); }
        }

        public static ObservableCollection<JavaPath> JavaPathList {
            get {
                if (_config == null) Load();
                if (javaPathList.Count == 0 && _config.TryGetValue("javaPathList", out var obj) && obj is object[] arr) {
                    foreach (var item in arr) {
                        if (item is Dictionary<string, object> dict) {
                            string path = dict.TryGetValue("path", out var p) ? p?.ToString() ?? "" : "";
                            string version = dict.TryGetValue("version", out var v) ? v?.ToString() ?? "" : "";
                            javaPathList.Add(new JavaPath(path, version));
                        }
                    }
                }
                if (!_javaPathListEventRegistered) {
                    javaPathList.CollectionChanged += (s, e) => {
                        if (_config == null) Load();
                        var saveArr = new object[javaPathList.Count];
                        for (int i = 0; i < javaPathList.Count; i++) {
                            saveArr[i] = new Dictionary<string, object> {
                                ["path"] = javaPathList[i].Path,
                                ["version"] = javaPathList[i].Version
                            };
                        }
                        _config["javaPathList"] = saveArr;
                        Save();
                    };
                    _javaPathListEventRegistered = true;
                }
                return javaPathList;
            }
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
                    { "javaPath", "" },
                    { "javaPathList", new object[0] }
                };

            string json = new JavaScriptSerializer().Serialize(_config);
            File.WriteAllText(ConfigPath, json);
        }
    }
}

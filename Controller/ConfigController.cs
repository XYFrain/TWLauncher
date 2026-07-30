using System.Collections.Generic;
using TWLauncher.Service;

namespace TWLauncher.Controller {
    /// <summary>
    /// 配置入口（静态）：外部唯一的配置访问点，内部委托给 ConfigService。
    /// </summary>
    internal static class ConfigController {
        /// <summary>当前选中的 Java bin 目录。</summary>
        public static string JavaPath {
            get => ConfigService.JavaPath;
            set => ConfigService.JavaPath = value;
        }

        /// <summary>JVM 最大内存（MB）。</summary>
        public static int MaxMemory {
            get => ConfigService.MaxMemory;
            set => ConfigService.MaxMemory = value;
        }

        /// <summary>玩家名。</summary>
        public static string PlayerName {
            get => ConfigService.PlayerName;
            set => ConfigService.PlayerName = value;
        }

        /// <summary>已检测到的 Java 列表 [{path, version}]。</summary>
        public static List<Dictionary<string, string>> JavaList {
            get => ConfigService.JavaList;
            set => ConfigService.JavaList = value;
        }

        /// <summary>返回拼装好的显示条目。</summary>
        public static List<(string path, string displayName)> GetJavaEntries()
            => ConfigService.GetJavaEntries();


        /// <summary>从磁盘加载配置到内存。</summary>
        public static void Load() => ConfigService.Load();
        /// <summary>把内存中的配置写回磁盘。</summary>
        public static void Save() => ConfigService.Save();
    }
}

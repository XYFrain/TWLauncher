using System.Collections.ObjectModel;
using TWLauncher.Service;
using TWLauncher.Models;

namespace TWLauncher.Controller {
    internal static class ConfigController {
        public static string PlayerName {
            get => ConfigService.PlayerName;
            set => ConfigService.PlayerName = value;
        }
        public static int MaxMemory {
            get => ConfigService.MaxMemory;
            set => ConfigService.MaxMemory = value;
        }
        public static string JavaPath {
            get => ConfigService.JavaPath;
            set => ConfigService.JavaPath = value;
        }
        public static ObservableCollection<JavaPath> JavaPathList {
            get => ConfigService.JavaPathList;
        }

        public static void Load() => ConfigService.Load();
        public static void Save() => ConfigService.Save();
    }
}

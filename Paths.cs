using System;
using System.IO;

namespace TWLauncher {
    internal static class Paths {
        private static readonly string _root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Game");

        public static string GameRoot => _root;

        public static string MinecraftJson => Path.Combine(_root, "Minecraft.json");

        public static string Minecraftjar => Path.Combine(_root, "Minecraft.jar");

        public static string LibrariesPath => Path.Combine(_root, "libraries");

        public static string NativesPath => Path.Combine(_root, "natives");

        public static string AssetsPath => Path.Combine(_root, "assets");

        public static string AssetsIndexesJson => Path.Combine(AssetsPath, "indexes", "5.json");

        public static string AssetsObjectsPath => Path.Combine(AssetsPath, "objects");

        public static string LauncherConfigPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using TWLauncher.Models;
using TWLauncher.Utils;

namespace TWLauncher.Service {
    /// <summary>
    /// 资源完整性检测服务（静态单例）：扫描本地文件，SHA1 比对，结果保存在自身静态属性中。
    /// </summary>
    internal static class CheckService {

        // 待下载 / 缺失的资源文件列表
        public static List<ResourceItem> Items { get; private set; } = new List<ResourceItem>();
        // 扫描的文件总数（包括已存在的）
        public static int TotalCount { get; private set; }
        // 所有缺失文件的总字节数
        public static long TotalBytes { get; private set; }

        /// <summary>
        /// 扫描所有本地文件（jar / libraries / natives / assets），
        /// 通过 SHA1 比对，结果写入自身静态属性。
        /// </summary>
        public static void ScanResources() {
            Items = new List<ResourceItem>();
            TotalCount = 0;
            TotalBytes = 0;
            var minecraftJson = JsonUtil.ReadJson(Paths.MinecraftJson);
            var assetsIndexesJson = JsonUtil.ReadJson(Paths.AssetsIndexesJson);

            CheckMinecraftJar(minecraftJson);
            CheckLibraries(minecraftJson);
            CheckAssets(assetsIndexesJson);
        }

        // ===================== 各资源类型扫描 =====================

        private static void CheckMinecraftJar(Dictionary<string, object> minecraftJson) {
            LogUtil.Info("---- [Minecraft.jar] ----");
            string sha1;
            long size;
            string clientUrl;
            if (!JsonUtil.TryGetDict(minecraftJson, "downloads", out var downloads)
                || !JsonUtil.TryGetDict(downloads, "client", out Dictionary<string, object> clientJson)
                || !JsonUtil.TryGetString(clientJson, "sha1", out sha1)
                || !JsonUtil.TryGetInt64(clientJson, "size", out size))
                return;
            JsonUtil.TryGetString(clientJson, "url", out clientUrl);

            TotalCount++;
            if (!FileUtil.HashVerify(Paths.Minecraftjar, sha1)) {
                TotalBytes += size;
                Items.Add(new ResourceItem
                {
                    Url = clientUrl,
                    Path = Paths.Minecraftjar,
                    Sha1 = sha1,
                    Size = size
                });
                LogUtil.Info(string.Format("[缺少] Game{0}", Paths.Minecraftjar.Substring(Paths.GameRoot.Length)));
            }
        }

        private static void CheckLibraries(Dictionary<string, object> minecraftJson) {
            LogUtil.Info("---- [Libraries] ----");
            object[] libraries;
            if (!JsonUtil.TryGetArray(minecraftJson, "libraries", out libraries))
                return;

            foreach (object libValue in libraries) {
                var library = libValue as Dictionary<string, object>;
                if (!RuleUtil.IsAllowedOnWindows(library))
                    continue;

                string path = null;
                string sha1 = null;
                long size = 0;
                string url = null;

                // 1. 原版格式：downloads.artifact
                if (JsonUtil.TryGetDict(library, "downloads", out var downloads)
                    && JsonUtil.TryGetDict(downloads, "artifact", out var artifact)) {
                    JsonUtil.TryGetString(artifact, "path", out path);
                    JsonUtil.TryGetString(artifact, "sha1", out sha1);
                    JsonUtil.TryGetInt64(artifact, "size", out size);
                    JsonUtil.TryGetString(artifact, "url", out url);
                }

                // 2. Fabric 格式：路径从 name 拼，sha1/size/url 在顶层
                if (string.IsNullOrEmpty(path)) {
                    string libName;
                    if (!JsonUtil.TryGetString(library, "name", out libName))
                        continue;
                    string[] parts = libName.Split(':');
                    if (parts.Length != 3) continue;
                    path = string.Format("{0}/{1}/{2}/{1}-{2}.jar",
                        parts[0].Replace('.', '/'), parts[1], parts[2]);
                    JsonUtil.TryGetString(library, "sha1", out sha1);
                    JsonUtil.TryGetInt64(library, "size", out size);
                    JsonUtil.TryGetString(library, "url", out url);
                }

                string destPath = Path.Combine(Paths.LibrariesPath, path);
                TotalCount++;

                bool missing = string.IsNullOrEmpty(sha1)
                    ? !File.Exists(destPath)
                    : !FileUtil.HashVerify(destPath, sha1);

                if (missing) {
                    TotalBytes += size;
                    if (string.IsNullOrEmpty(url)) {
                        LogUtil.Info(string.Format("[缺少] 无下载地址，跳过: Game{0}", destPath.Substring(Paths.GameRoot.Length)));
                        continue;
                    }
                    // url 以 / 结尾的是 Maven 仓库根地址（如 Fabric），需拼接 path
                    if (url.EndsWith("/"))
                        url = url + path;
                    Items.Add(new ResourceItem {
                        Url = url,
                        Path = destPath,
                        Sha1 = sha1 ?? "",
                        Size = size
                    });
                    LogUtil.Info(string.Format("[缺少] Game{0}", destPath.Substring(Paths.GameRoot.Length)));
                }
            }
        }

        private static void CheckAssets(Dictionary<string, object> assetsIndexesJson) {
            LogUtil.Info("---- [Assets] ----");
            Dictionary<string, object> assetObjects;
            if (!JsonUtil.TryGetDict(assetsIndexesJson, "objects", out assetObjects))
                return;

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, object> entry in assetObjects) {
                var asset = entry.Value as Dictionary<string, object>;
                string hash;
                long size;
                if (!JsonUtil.TryGetString(asset, "hash", out hash)
                    || !JsonUtil.TryGetInt64(asset, "size", out size))
                    continue;

                if (!seen.Add(hash))
                    continue;

                if (hash.Length < 2) continue;
                string subDir = hash.Substring(0, 2);
                string assetPath = Path.Combine(Paths.AssetsObjectsPath, subDir, hash);
                TotalCount++;
                if (!FileUtil.HashVerify(assetPath, hash)) {
                    TotalBytes += size;
                    Items.Add(new ResourceItem
                    {
                        Url = string.Format("https://resources.download.minecraft.net/{0}/{1}", subDir, hash),
                        Path = assetPath,
                        Sha1 = hash,
                        Size = size
                    });
                    LogUtil.Info(string.Format("[缺少] Game{0}", assetPath.Substring(Paths.GameRoot.Length)));
                }
            }
        }

    }
}

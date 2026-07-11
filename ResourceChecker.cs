using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace TWlauncher
{
    internal enum GameActionState
    {
        Download,
        Update,
        Ready
    }

    internal sealed class ResourceChecker
    {
        private static readonly string GameRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Game");

        /// <summary>
        /// 验证游戏资源完成性
        /// </summary>
        public GameActionState Check()
        {
            if (!CheckBaseResources())
            {
                return GameActionState.Download;
            }

            return CheckCustomResources()
                ? GameActionState.Ready
                : GameActionState.Update;
        }



        /// <summary>
        /// 检测基础游戏资源
        /// </summary>
        public bool CheckBaseResources()
        {
            string minecraftJsonPath = Path.Combine(GameRoot, "Minecraft.json");
            if (!File.Exists(minecraftJsonPath))
                return false;
            string minecraftjarPath = Path.Combine(GameRoot, "Minecraft.jar");

            // 检测Minecraft.jar、Libraries、Natives、Assets文件完整性
            try
            {
                // 检测Minecraft.jar
                Dictionary<string, object> minecraftJsonMetadata = JsonUtility.ReadJsonObject(minecraftJsonPath);
                Dictionary<string, object> downloads;
                Dictionary<string, object> client;
                if (!JsonUtility.TryGetJsonObject(minecraftJsonMetadata, "downloads", out downloads)
                    || !JsonUtility.TryGetJsonObject(downloads, "client", out client)
                    || !VerifyFile(minecraftjarPath, client, "sha1"))
                {
                    return false;
                }

                // 检测Libraries、Natives、Assets
                return CheckLibraries(minecraftJsonMetadata) && CheckNatives(minecraftJsonMetadata) && CheckAssets(minecraftJsonMetadata);
            }catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
            catch (ArgumentException) { return false; }
            }

        /// <summary>
        /// 检查适用于 Windows 的依赖库文件是否完整。
        /// </summary>
        private static bool CheckLibraries(Dictionary<string, object> minecraftJsonMetadata)
        {
            // 获取所有libraries
            object[] libraries;
            if (!JsonUtility.TryGetJsonArray(minecraftJsonMetadata, "libraries", out libraries))
                return false;

            foreach (object libraryValue in libraries)
            {
                Dictionary<string, object> library = libraryValue as Dictionary<string, object>;
                // 判断如果此library不适用于windows就跳过
                if (!IsLibraryAllowedOnWindows(library))
                    continue;

                Dictionary<string, object> downloads;
                if (!JsonUtility.TryGetJsonObject(library, "downloads", out downloads))
                    return false;


                // 检查普通artifact jar是否存在
                Dictionary<string, object> artifact;
                string libraryPath;
                if (JsonUtility.TryGetJsonObject(downloads, "artifact", out artifact))
                {
                    if (!JsonUtility.TryGetJsonString(artifact, "path", out libraryPath)
                        || !VerifyFile(Path.Combine(GameRoot, "libraries", libraryPath), artifact, "sha1"))
                        return false;
                }

                // 检查windows native classifier jar是否存在
                Dictionary<string, object> natives;
                string classifierName;
                if (!JsonUtility.TryGetJsonObject(library, "natives", out natives) || !JsonUtility.TryGetJsonString(natives, "windows", out classifierName))
                    continue;

                Dictionary<string, object> classifiers;
                Dictionary<string, object> nativeArtifact;
                string nativePath;

                if (!JsonUtility.TryGetJsonObject(downloads, "classifiers", out classifiers)
                    || !JsonUtility.TryGetJsonObject(classifiers, classifierName, out nativeArtifact)
                    || !JsonUtility.TryGetJsonString(nativeArtifact, "path", out nativePath)
                    || !VerifyFile(Path.Combine(GameRoot, "libraries", nativePath), nativeArtifact, "sha1"))
                    return false;
            }

            return true;
        }
        /// <summary>
        /// 检查适用于 Windows 的本地依赖文件是否已完整解压到 natives 文件夹。
        /// </summary>
        private static bool CheckNatives(Dictionary<string, object> versionMetadata)
        {
            // 获取所有libraries
            object[] libraries;
            if (!JsonUtility.TryGetJsonArray(versionMetadata, "libraries", out libraries))
                return false;

            string nativesPath = Path.Combine(GameRoot, "natives");
            foreach (object libraryValue in libraries)
            {
                Dictionary<string, object> library = libraryValue as Dictionary<string, object>;

                // 判断如果此library格式错误或不适用于windows就跳过
                if (!IsLibraryAllowedOnWindows(library))
                    continue;

                // 如果此library没有windows本地依赖就跳过
                Dictionary<string, object> natives;
                string classifierName;
                if (!JsonUtility.TryGetJsonObject(library, "natives", out natives) || !JsonUtility.TryGetJsonString(natives, "windows", out classifierName))
                    continue;


                // 获取windows本地依赖对应的jar路径
                Dictionary<string, object> downloads;
                Dictionary<string, object> classifiers;
                Dictionary<string, object> nativeArtifact;
                string nativePath;
                if (!JsonUtility.TryGetJsonObject(library, "downloads", out downloads) || !JsonUtility.TryGetJsonObject(downloads, "classifiers", out classifiers) || !JsonUtility.TryGetJsonObject(classifiers, classifierName, out nativeArtifact) || !JsonUtility.TryGetJsonString(nativeArtifact, "path", out nativePath))
                    return false;

                // 读取native jar中的文件清单，用于核对natives文件夹中的解压结果
                string nativeJarPath = Path.Combine(GameRoot, "libraries", nativePath);

                // 获取解压时需要排除的文件或文件夹，例如META-INF/
                List<string> excludedPaths = new List<string>();
                Dictionary<string, object> extract;
                object[] excludes;
                if (JsonUtility.TryGetJsonObject(library, "extract", out extract) && JsonUtility.TryGetJsonArray(extract, "exclude", out excludes))
                    foreach (object excludeValue in excludes)
                    {
                        string excludedPath = excludeValue as string;
                        if (!string.IsNullOrEmpty(excludedPath))
                            excludedPaths.Add(excludedPath);
                    }

                // 根据native jar中的文件清单，逐个检查natives文件夹中的解压结果
                using (System.IO.Compression.ZipArchive archive = System.IO.Compression.ZipFile.OpenRead(nativeJarPath))
                {
                    foreach (System.IO.Compression.ZipArchiveEntry entry in archive.Entries)
                    {
                        // Name为空表示该条目是文件夹，不需要检查
                        if (string.IsNullOrEmpty(entry.Name))
                            continue;

                        string entryPath = entry.FullName;
                        bool excluded = false;
                        foreach (string excludedPath in excludedPaths)
                        {
                            if (entryPath.StartsWith(excludedPath, StringComparison.OrdinalIgnoreCase))
                            {
                                excluded = true;
                                break;
                            }
                        }
                        if (excluded)
                            continue;

                        string extractedPath = Path.Combine(nativesPath, entryPath.Replace('/', Path.DirectorySeparatorChar));
                        if (!File.Exists(extractedPath)
                            || new FileInfo(extractedPath).Length != entry.Length)
                            return false;
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// 检查资源索引以及assets/objects中的资源文件是否完整。
        /// </summary>
        private static bool CheckAssets(Dictionary<string, object> versionMetadata)
        {
            // 获取资源索引信息
            Dictionary<string, object> assetIndex;
            string assetIndexId;
            if (!JsonUtility.TryGetJsonObject(versionMetadata, "assetIndex", out assetIndex)
                || !JsonUtility.TryGetJsonString(assetIndex, "id", out assetIndexId)
                || string.IsNullOrWhiteSpace(assetIndexId))
            {
                return false;
            }

            // 检查资源索引文件的存在性、大小和SHA-1
            string assetIndexPath = Path.Combine(GameRoot, "assets", "indexes", assetIndexId + ".json");
            if (!VerifyFile(assetIndexPath, assetIndex, "sha1"))
                return false;

            Dictionary<string, object> assetIndexMetadata = JsonUtility.ReadJsonObject(assetIndexPath);
            Dictionary<string, object> assetObjects;
            if (!JsonUtility.TryGetJsonObject(assetIndexMetadata, "objects", out assetObjects))
                return false;

            // 相同hash表示同一个物理资源文件，只需要校验一次
            HashSet<string> verifiedHashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, object> entry in assetObjects)
            {
                Dictionary<string, object> asset = entry.Value as Dictionary<string, object>;
                string hash;
                if (asset == null
                    || !JsonUtility.TryGetJsonString(asset, "hash", out hash)
                    || hash.Length < 2)
                {
                    return false;
                }

                if (!verifiedHashes.Add(hash))
                    continue;

                string assetPath = Path.Combine(
                    GameRoot,
                    "assets",
                    "objects",
                    hash.Substring(0, 2),
                    hash);
                if (!VerifyFile(assetPath, asset, "hash"))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 根据 Library 的 rules 规则判断该库是否适用于 Windows。
        /// 没有规则的库默认适用于所有平台。
        /// </summary>
        private static bool IsLibraryAllowedOnWindows(Dictionary<string, object> library)
        {
            object[] rules;
            // 如果没有 rules，该库默认适用于所有系统
            if (!JsonUtility.TryGetJsonArray(library, "rules", out rules))
                return true;

            // 存在 rules 时，默认不允许；后面的匹配规则覆盖前面的结果
            bool allowed = false;
            foreach (object ruleValue in rules)
            {
                Dictionary<string, object> rule = ruleValue as Dictionary<string, object>;

                string action;
                if (!JsonUtility.TryGetJsonString(rule, "action", out action))
                    continue;

                Dictionary<string, object> os;

                // 有 os 条件时，仅处理 Windows 规则
                if (JsonUtility.TryGetJsonObject(rule, "os", out os))
                {
                    string name;

                    if (!JsonUtility.TryGetJsonString(os, "name", out name))
                        continue;

                    if (!string.Equals(name, "windows", StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                // 没有 os 表示适用于所有系统；有 os 则已经确认是 Windows
                if (string.Equals(action, "allow", StringComparison.OrdinalIgnoreCase))
                    allowed = true;
                else if (string.Equals(action, "disallow", StringComparison.OrdinalIgnoreCase))
                    allowed = false;
            }
            return allowed;
        }
        /// <summary>
        /// 根据JSON中的文件大小和哈希值检查本地文件是否完整。
        /// </summary>
        private static bool VerifyFile(string filePath, Dictionary<string, object> fileMetadata, string hashKey)
        {
            long expectedSize;
            string expectedSha1;
            if (!JsonUtility.TryGetJsonInt64(fileMetadata, "size", out expectedSize)
                || !JsonUtility.TryGetJsonString(fileMetadata, hashKey, out expectedSha1))
            {
                return false;
            }

            if (!File.Exists(filePath))
                return false;

            if (new FileInfo(filePath).Length != expectedSize)
                return false;

            using (FileStream stream = File.OpenRead(filePath))
            using (System.Security.Cryptography.SHA1 sha1 =
                System.Security.Cryptography.SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(stream);
                string actualSha1 = BitConverter.ToString(hash).Replace("-", string.Empty);
                return string.Equals(actualSha1, expectedSha1, StringComparison.OrdinalIgnoreCase);
            }
        }



        /// <summary>
        /// 检测自定义游戏资源
        /// </summary>
        public bool CheckCustomResources()
        {
            return true;
        }

    }
}

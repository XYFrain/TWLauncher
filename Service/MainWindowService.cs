using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using TWLauncher.Controller;
using TWLauncher.Utils;

namespace TWLauncher.Service {
    /// <summary>
    /// 主窗口业务服务（静态）：组装 DownloadService 和 LaunchService，提供统一的游戏操作入口。
    /// </summary>
    internal static class MainWindowService {

        /// <summary>网络连通检测，不通则弹窗退出。</summary>
        public static async Task CheckNetworkAsync() {
            LogUtil.Info("[网络] 开始检测...");
            try {
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) })
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5))) {
                    var request = new HttpRequestMessage(HttpMethod.Head, "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json");
                    var response = await client.SendAsync(request, cts.Token);
                    if (!response.IsSuccessStatusCode) {
                        LogUtil.Error("[网络] 服务器返回错误: " + response.StatusCode);
                        MessageBox.Show("无法连接到资源服务器，请检查网络后重试。", "网络错误",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current?.Shutdown();
                    }
                }
            } catch {
                LogUtil.Error("[网络] 连接失败");
                MessageBox.Show("无法连接到资源服务器，请检查网络后重试。", "网络错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current?.Shutdown();
            }
            LogUtil.Info("[网络] 检测通过");
        }

        /// <summary>检测已安装 Java，配置文件有则复用，无则全盘扫描。</summary>
        public static async Task CheckJavaAsync() {
            LogUtil.Info("[Java] 开始检测...");
            var javaList = ConfigController.JavaPathList;
            if (javaList.Count == 0) {
                LogUtil.Info("[Java] 配置无缓存，开始全盘扫描...");
                var detected = await JavaService.CheckAsync();
                foreach (var java in detected)
                    javaList.Add(java);
                if (javaList.Count == 0) {
                    LogUtil.Error("[Java] 未检测到 Java 17+");
                    var choice = MessageBox.Show("未检测到 Java 17+，是否手动选择？", "Java 缺失",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (choice == MessageBoxResult.No) {
                        Application.Current?.Shutdown();
                        return;
                    }

                    var validated = JavaUtil.PickJava();
                    if (validated == null) {
                        Application.Current?.Shutdown();
                        return;
                    }
                    javaList.Add(validated);
                    SettingsViewModel.Instance.JavaPath = validated.Path;
                    LogUtil.Info(string.Format("[Java] 用户手动选择: {0}", validated.Path));
                }
                LogUtil.Info(string.Format("[Java] 扫描完成，找到 {0} 个", javaList.Count));
            } else
                LogUtil.Info(string.Format("[Java] 复用配置缓存，{0} 个", javaList.Count));

            // 如果当前 javaPath 为空，自动填入第一个
            if (string.IsNullOrEmpty(ConfigController.JavaPath) && javaList.Count > 0) {
                SettingsViewModel.Instance.JavaPath = javaList[0].Path;
                LogUtil.Info(string.Format("[Java] 自动选择第一个 Java: {0}", javaList[0].Path));
            }
        }

        /// <summary>下载并合并 JSON，生成 Minecraft.json 与资源索引。</summary>
        public static async Task CheckJsonAsync() {
            if (File.Exists(Paths.MinecraftJson) && File.Exists(Paths.AssetsIndexesJson)) {
                LogUtil.Info("[JSON] 文件已存在，跳过下载");
                return;
            }
            LogUtil.Info("[JSON] 开始下载...");
            try {
                // 1. 获取 Fabric loader 最新版本
                LogUtil.Info("[JSON] 获取 Fabric loader 版本...");
                string loaderMetaUrl = string.Format("https://meta.fabricmc.net/v2/versions/loader/{0}", "1.20.1");
                string loaderJson = await HttpUtil.GetString(loaderMetaUrl, CancellationToken.None);
                var loaderArr = new JavaScriptSerializer().DeserializeObject(loaderJson) as object[];
                string loaderVersion = ((loaderArr[0] as Dictionary<string, object>)["loader"]
                    as Dictionary<string, object>)["version"].ToString();
                LogUtil.Info("[JSON] Fabric loader: " + loaderVersion);


                // 2. 下载原版 1.20.1 JSON（先获取版本清单，再定位具体版本 URL）
                LogUtil.Info("[JSON] 下载版本清单...");
                string manifestUrl = "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json";
                string manifestJson = await HttpUtil.GetString(manifestUrl, CancellationToken.None);
                var manifest = new JavaScriptSerializer().DeserializeObject(manifestJson) as Dictionary<string, object>;
                var versions = (object[])manifest["versions"];
                string vanillaUrl = null;
                foreach (object v in versions) {
                    var versionEntry = v as Dictionary<string, object>;
                    if (string.Equals(versionEntry["id"] as string, "1.20.1", StringComparison.OrdinalIgnoreCase)) {
                        vanillaUrl = versionEntry["url"] as string;
                        break;
                    }
                }
                if (vanillaUrl == null) throw new Exception("在版本清单中未找到 1.20.1");
                LogUtil.Info("[JSON] 下载原版 JSON...");
                string vanillaJson = await HttpUtil.GetString(vanillaUrl, CancellationToken.None);
                var vanilla = new JavaScriptSerializer().DeserializeObject(vanillaJson) as Dictionary<string, object>;
                LogUtil.Info("[JSON] 原版 JSON 下载完成");


                // 3. 下载 Fabric profile JSON
                LogUtil.Info("[JSON] 下载 Fabric profile JSON...");
                string fabricUrl = string.Format("https://meta.fabricmc.net/v2/versions/loader/{0}/{1}/profile/json","1.20.1", loaderVersion);
                string fabricJson = await HttpUtil.GetString(fabricUrl, CancellationToken.None);
                var fabric = new JavaScriptSerializer().DeserializeObject(fabricJson) as Dictionary<string, object>;
                LogUtil.Info("[JSON] Fabric profile 下载完成");


                // 4. 合并保存到 Minecraft.json
                LogUtil.Info("[JSON] 合并...");
                LogUtil.Info("[JSON] 合并: 覆写 id / mainClass");
                vanilla["id"] = fabric["id"];
                vanilla["mainClass"] = fabric["mainClass"];
                LogUtil.Info("[JSON] 合并: 拼接 libraries");
                if (JsonUtil.TryGetArray(vanilla, "libraries", out var vLibs) && JsonUtil.TryGetArray(fabric, "libraries", out var fLibs)) {
                    LogUtil.Info(string.Format("[JSON] 合并: libraries vanilla {0} + fabric {1}", vLibs.Length, fLibs.Length));
                    var mergedLibs = new object[vLibs.Length + fLibs.Length];
                    vLibs.CopyTo(mergedLibs, 0);
                    fLibs.CopyTo(mergedLibs, vLibs.Length);
                    vanilla["libraries"] = mergedLibs;
                } else
                    LogUtil.Info("[JSON] 合并: libraries 合并跳过");
                LogUtil.Info("[JSON] 合并: 拼接 arguments.jvm");
                if (JsonUtil.TryGetDict(vanilla, "arguments", out var vArgs) && JsonUtil.TryGetDict(fabric, "arguments", out var fArgs) && JsonUtil.TryGetArray(vArgs, "jvm", out var vJvm) && JsonUtil.TryGetArray(fArgs, "jvm", out var fJvm)) {
                    LogUtil.Info(string.Format("[JSON] 合并: jvm vanilla {0} + fabric {1}", vJvm.Length, fJvm.Length));
                    var mergedJvm = new object[vJvm.Length + fJvm.Length];
                    vJvm.CopyTo(mergedJvm, 0);
                    fJvm.CopyTo(mergedJvm, vJvm.Length);
                    vArgs["jvm"] = mergedJvm;
                } else
                    LogUtil.Info("[JSON] 合并: arguments 合并跳过");
                LogUtil.Info("[JSON] 合并: 清理冗余字段");
                vanilla.Remove("releaseTime");
                vanilla.Remove("time");
                vanilla.Remove("type");
                vanilla.Remove("minimumLauncherVersion");
                vanilla.Remove("assets");
                vanilla.Remove("complianceLevel");
                LogUtil.Info("[JSON] 合并: 序列化保存...");
                string mergedJson = new JavaScriptSerializer().Serialize(vanilla);
                Directory.CreateDirectory(Paths.GameRoot);
                File.WriteAllText(Paths.MinecraftJson, mergedJson);
                LogUtil.Info("[JSON] Minecraft.json 已保存");

                // 5. 下载资源索引 JSON
                LogUtil.Info("[JSON] 下载资源索引...");
                var assetIndex = (Dictionary<string, object>)vanilla["assetIndex"];
                string assetId = assetIndex["id"].ToString();
                string assetSha1 = assetIndex["sha1"].ToString();
                string assetIndexUrl = string.Format("https://piston-meta.mojang.com/v1/packages/{0}/{1}.json", assetSha1, assetId);
                string assetIndexJson = await HttpUtil.GetString(assetIndexUrl, CancellationToken.None);
                Directory.CreateDirectory(Path.GetDirectoryName(Paths.AssetsIndexesJson));
                File.WriteAllText(Paths.AssetsIndexesJson, assetIndexJson);
                LogUtil.Info("[JSON] 资源索引下载完成");
            } catch (Exception ex) {
                LogUtil.Error("[JSON] 下载失败: " + ex.Message);
                MessageBox.Show("游戏资源下载失败，请检查网络后重试。\n" + ex.Message, "下载失败",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current?.Shutdown();
            }
        }

        // ===================== 资源完整性检测 =====================

        /// <summary>扫描本地文件 SHA1，根据结果切 Launch 或 Download 阶段。</summary>
        public static async Task CheckResourcesAsync() {
            LogUtil.Info("[资源] 开始完整性检测...");
            try {
                await Task.Run(() => CheckService.ScanResources());
                LogUtil.Info(string.Format("[资源] 扫描完成: 总数 {0}，需下载 {1}，共 {2}",
                    CheckService.TotalCount, CheckService.Items.Count,
                    ByteUtil.Format(CheckService.TotalBytes)));

                if (CheckService.Items.Count == 0) {
                    LogUtil.Info("[资源] 全部就绪，可启动");
                    MainButtonViewModel.Instance.SetPhase(MainButtonViewModel.Phase.Launch);
                } else {
                    MainButtonViewModel.Instance.SetPhase(MainButtonViewModel.Phase.Download);
                }
            } catch (Exception ex) {
                LogUtil.Error("[资源] 检测失败: " + ex.Message);
                MessageBox.Show("资源完整性检测失败，请检查游戏文件后重试。\n" + ex.Message, "检测失败",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current?.Shutdown();
            }
        }
    }
}
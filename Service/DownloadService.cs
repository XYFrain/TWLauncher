using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TWLauncher.Models;
using TWLauncher.Utils;

namespace TWLauncher.Service {
    /// <summary>
    /// 下载服务：并发下载资源文件。
    /// </summary>
    internal static class DownloadService {
        /// <summary>
        /// 并发下载 CheckService 清单中的所有资源文件。
        /// </summary>
        public static async Task DownloadResourcesAsync(IProgress<ProgressViewModel> progress, CancellationTokenSource cts) {
            var ct = cts.Token; // 取消令牌
            var items = CheckService.Items; // 缺失文件下载列表 
            long totalBytes = CheckService.TotalBytes; // 所有缺失文件的总字节数
            int totalFiles = items.Count; // 总共多少个文件要下载
            long downloadBytes = 0; // 已下载字节数
            int downloadFiles = 0; // 已完成文件数

            LogUtil.Info(string.Format("[下载] 开始下载，共 {0} 个文件, {1}", totalFiles, ByteUtil.Format(totalBytes)));

            // 最多 8 个并发下载，完成后逐个补上，始终保持 ≤8 个活跃 Task
            var running = new List<Task>(); // 当前正在跑的任务，最多 8 个
            var runningItems = new Dictionary<Task, ResourceItem>(); // 每个 Task 对应它的文件大小
            var firstError = null as Exception; // 记录第一个下载失败的错误
            int index = 0; // 指针，指向 items 中下一个待启动的文件

            while (index < items.Count || running.Count > 0) {
                ct.ThrowIfCancellationRequested();
                // 补满到 8 个且还有待下载文件
                while (running.Count < 8 && index < items.Count) {
                    ResourceItem item = items[index++];
                    Task task = HttpUtil.DownloadToFile(item.Url, item.Path, ct);
                    running.Add(task);
                    runningItems[task] = item;
                }
                // 没有任务在跑了 → 结束
                if (running.Count == 0)
                    break;
                // 等任意一个完成
                Task doneTask = await Task.WhenAny(running);
                running.Remove(doneTask);
                ResourceItem doneItem;
                runningItems.TryGetValue(doneTask, out doneItem);
                runningItems.Remove(doneTask);
                long size = doneItem != null ? doneItem.Size : 0;

                try {
                    await doneTask;
                } catch (OperationCanceledException) {
                    if (firstError == null) {
                        LogUtil.Info("[下载] 用户取消");
                        throw;
                    }
                } catch (Exception ex) {
                    string failedUrl = doneItem != null ? doneItem.Url : "?";
                    string failedPath = doneItem != null ? doneItem.Path : "?";
                    LogUtil.Error(string.Format("[下载] 任务失败: {0} URL={1} Path={2}", ex.Message, failedUrl, failedPath));
                    if (firstError == null) {
                        firstError = ex;
                        LogUtil.Info("[下载] 正在取消其余任务...");
                        cts.Cancel(false);
                    }
                }

                if (firstError == null) {
                    long current = downloadBytes += size;
                    int currentFiles = ++downloadFiles;
                    progress.Report(new ProgressViewModel {
                        Percent = totalFiles > 0 ? currentFiles * 100.0 / totalFiles : 100,
                        StageText = string.Format("正在下载 {0}/{1} 文件  {2}/{3}",
                            currentFiles, totalFiles, ByteUtil.Format(current), ByteUtil.Format(totalBytes)),
                        IsActive = true
                    });
                }
            }

            if (firstError != null) {
                LogUtil.Error(string.Format("[下载] 中止: {0}", firstError.Message));
                throw firstError;
            }

            LogUtil.Info("[下载] 全部文件下载完成");
        }
    }
}

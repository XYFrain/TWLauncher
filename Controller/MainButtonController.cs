using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TWLauncher.Service;
using TWLauncher.Utils;

namespace TWLauncher.Controller {
    /// <summary>
    /// 主按钮流程控制器（静态）：处理按钮点击后的下载 / 启动 / 取消流程。
    /// </summary>
    internal static class MainButtonController {
        private static CancellationTokenSource cts;
        private static bool busy;

        /// <summary>根据当前按钮状态分发到对应流程。</summary>
        public static async void HandleMainButton() {
            // 取消下载随时可以，不拦
            if (MainButtonViewModel.Instance.Current == MainButtonViewModel.Phase.Downloading) {
                Cancel();
                return;
            }
            // 其他操作进行中，忽略重复点击
            if (busy) return;
            busy = true;
            try {
                switch (MainButtonViewModel.Instance.Current) {
                    case MainButtonViewModel.Phase.Launch:
                        await LaunchAsync();
                        break;
                    case MainButtonViewModel.Phase.Download:
                        await DownloadAsync();
                        break;
                    case MainButtonViewModel.Phase.Update:
                        await UpdateAsync();
                        break;
                }
            } finally {
                busy = false;
            }
        }

        private static Task LaunchAsync() {
            try {
                LaunchService.Launch();
            } catch (Exception ex) {
                MessageBox.Show("启动失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return Task.CompletedTask;
        }

        private static async Task DownloadAsync() {
            var tokenSource = new CancellationTokenSource();
            cts = tokenSource;
            MainButtonViewModel.Instance.SetPhase(MainButtonViewModel.Phase.Downloading);
            ProgressViewModel.Instance.Visibility = Visibility.Visible;
            ProgressViewModel.Instance.StageText = "准备下载...";
            ProgressViewModel.Instance.Percent = 0;

            try {
                var reporter = new Progress<ProgressViewModel>(p => {
                    ProgressViewModel.Instance.Percent = p.Percent;
                    ProgressViewModel.Instance.StageText = p.StageText;
                });
                await DownloadService.DownloadResourcesAsync(reporter, tokenSource);
                MainButtonViewModel.Instance.SetPhase(MainButtonViewModel.Phase.Launch);
            } catch (OperationCanceledException) {
                MainButtonViewModel.Instance.SetPhase(MainButtonViewModel.Phase.Download);
            } catch (Exception ex) {
                MessageBox.Show("下载失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                MainButtonViewModel.Instance.SetPhase(MainButtonViewModel.Phase.Download);
            } finally {
                ProgressViewModel.Instance.Visibility = Visibility.Collapsed;
                if (cts == tokenSource) cts = null;
            }
        }

        private static Task UpdateAsync() {
            return Task.CompletedTask;
        }

        private static void Cancel() {
            LogUtil.Info("[Cancel] 用户点击取消");
            if (cts != null) {
                cts.Cancel(false);   // false: 回调走线程池，不阻塞 UI
            }
        }

    }
}
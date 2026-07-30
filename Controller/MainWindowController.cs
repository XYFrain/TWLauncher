using System.Threading.Tasks;
using TWLauncher.Service;

namespace TWLauncher.Controller {
    /// <summary>
    /// 主窗口流程控制器：编排初始化流程。
    /// </summary>
    internal static class MainWindowController {

        /// <summary>异步初始化：网络检测 → Java 检测 → JSON 就绪 → 资源检测。</summary>
        public static async Task InitializeAsync() {
            MainButtonViewModel.Instance.SetPhase(MainButtonViewModel.Phase.Checking);

            // 1.网络连通检测，不通则弹窗退出
            await MainWindowService.CheckNetworkAsync();
            // 2.扫描已安装 Java，写入全局配置
            await MainWindowService.CheckJavaAsync();
            // 3.确保基础 JSON 就绪，不存在则下载
            await MainWindowService.CheckJsonAsync();

            // 4. 资源完整性检测（SHA1 比对），根据结果切 Launch / Download
            await MainWindowService.CheckResourcesAsync();
        }
    }
}

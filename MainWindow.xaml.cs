using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TWLauncher.Controller;

namespace TWLauncher {
    public partial class MainWindow : Window {
        internal MainWindow() {
            DataContext = MainViewModel.Instance;
            // 2. 按屏幕比例计算窗口大小并居中
            SetMainWindow(1.7, 0.6);
            // 3. 解析 XAML，画出所有控件
            InitializeComponent();
            // 4. 加载配置文件
            ConfigController.Load();
            // 5. 后台异步初始化（网络检测 → Java 检测 → JSON 就绪）
            MainWindowController.InitializeAsync().ContinueWith(t => {
                if (t.Exception != null)
                    Utils.LogUtil.Error(t.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        // ===================== 纯窗口操作 =====================
        private void DragMove(object sender, MouseButtonEventArgs e) => base.DragMove();
        private void Minimize(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Close(object sender, RoutedEventArgs e) => Close();
        // ===================== 窗口尺寸 =====================
        private void SetMainWindow(double windowRatio, double maxScale) {
            double screenWidth = SystemParameters.WorkArea.Width;
            double screenHeight = SystemParameters.WorkArea.Height;

            double maxWindowWidth = screenWidth * maxScale;
            double maxWindowHeight = screenHeight * maxScale;

            double winW, winH;
            double tempH = maxWindowWidth / windowRatio;
            if (tempH <= maxWindowHeight) {
                winW = maxWindowWidth;
                winH = tempH;
            } else {
                winH = maxWindowHeight;
                winW = winH * windowRatio;
            }

            Width = winW;
            Height = winH;
            Left = (screenWidth - winW) / 2;
            Top = (screenHeight - winH) / 2;
        }
    }
}

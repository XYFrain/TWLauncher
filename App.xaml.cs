using System.Threading;
using System.Windows;

namespace TWLauncher {
    public partial class App : Application {
        private Mutex _mutex;

        /// <summary>
        /// 应用程序启动入口。
        /// </summary>
        protected override void OnStartup(StartupEventArgs e) {
            CreateMutex();
            base.OnStartup(e);
            
            // 创建主界面，设置并显示
            MainWindow main = new MainWindow();
            main.Show();
        }

        /// <summary>
        /// 应用程序退出入口。
        /// </summary>
        protected override void OnExit(ExitEventArgs e) {
            DisposeMutex();
            base.OnExit(e);

        }





        /// <summary>
        /// 尝试创建单实例互斥体。
        /// </summary>
        /// <returns>如果当前进程是首个实例（成功创建）则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        private void CreateMutex() {
            string mutexName = "TWLauncher_SingleInstance_Mutex";
            bool createNew;
            _mutex = new Mutex(true, mutexName, out createNew);

            if (!createNew) {
                _mutex.Dispose();
                _mutex = null;
                MessageBox.Show("启动器已在运行，请勿重复打开！");
                Shutdown();
            }
        }
        /// <summary>
        /// 释放并销毁单实例互斥体。
        /// 若互斥体不存在（已释放或从未创建），则不执行任何操作。
        /// </summary>
        private void DisposeMutex() {
            if (_mutex != null) {
                _mutex.ReleaseMutex();
                _mutex.Dispose();
                _mutex = null;
            }
        }
    }
}

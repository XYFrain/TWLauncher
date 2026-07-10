using System;
using System.Threading;
using System.Windows;

namespace TWlauncher
{
    public partial class App : Application
    {
        private Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            // 程序唯一标识
            string mutexName = "TWLauncher_SingleInstance_Mutex";
            bool createNew;

            // 创建互斥体
            _mutex = new Mutex(true, mutexName, out createNew);

            if (!createNew)
            {
                _mutex = null;
                MessageBox.Show("启动器已在运行，请勿重复打开！");
                Shutdown();
                return;
            }

            base.OnStartup(e);
            MainWindow main = new MainWindow();
            main.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // 释放互斥锁
            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex.Dispose();
                _mutex = null;
            }
            base.OnExit(e);
        }
    }
}
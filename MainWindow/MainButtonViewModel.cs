using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TWLauncher {
    internal class MainButtonViewModel : INotifyPropertyChanged {
        public enum Phase { Checking, Launch, Download, Update, Downloading }
        public static MainButtonViewModel Instance { get; } = new MainButtonViewModel();

        // ===================== 构造函数 =====================
        private MainButtonViewModel() { }

        // ===================== 属性（XAML 绑定） =====================
        public Phase Current { get; private set; }
        public string MainButtonText {
            get => mainButtonText;
            set { mainButtonText = value; OnPropertyChanged(); }
        }
        private string mainButtonText;
        public bool MainButtonEnabled {
            get => mainButtonEnabled;
            set { mainButtonEnabled = value; OnPropertyChanged(); }
        }
        private bool mainButtonEnabled;

        // ===================== 命令（XAML 绑定） =====================

        // ===================== 方法 =====================
        public void SetPhase(Phase phase) {
            Current = phase;
            MainButtonEnabled = phase != Phase.Checking;
            switch (phase) {
                case Phase.Checking:    MainButtonText = "正在检查资源..."; break;
                case Phase.Launch:      MainButtonText = "启动游戏";       break;
                case Phase.Download:    MainButtonText = "下载游戏";       break;
                case Phase.Update:      MainButtonText = "更新游戏";       break;
                case Phase.Downloading: MainButtonText = "取消下载";       break;
            }
        }

        // ===================== INotifyPropertyChanged =====================
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TWLauncher {
    /// <summary>
    /// 主按钮 ViewModel（单例）：负责按钮文字和可用状态。
    /// </summary>
    internal class MainButtonViewModel : INotifyPropertyChanged {
        public enum Phase { Checking, Launch, Download, Update, Downloading }

        public static MainButtonViewModel Instance { get; } = new MainButtonViewModel();
        private MainButtonViewModel() { }

        /// <summary>当前阶段。</summary>
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

        /// <summary>切换阶段。</summary>
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
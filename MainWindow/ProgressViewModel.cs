using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace TWLauncher {
    /// <summary>
    /// 下载进度 ViewModel（单例），用于向 UI 报告当前下载状态。
    /// </summary>
    internal class ProgressViewModel : INotifyPropertyChanged {

        public static ProgressViewModel Instance { get; } = new ProgressViewModel();
        internal ProgressViewModel() { }

        /// <summary>当前下载百分比（0–100）。</summary>
        public double Percent {
            get => percent;
            set { percent = value; OnPropertyChanged(); }
        }
        private double percent;

        /// <summary>当前阶段描述文字。</summary>
        public string StageText {
            get => stageText;
            set { stageText = value; OnPropertyChanged(); }
        }
        private string stageText;

        /// <summary>下载速度文本，如"2.5 MB/s"。</summary>
        public string SpeedText {
            get => speedText;
            set { speedText = value; OnPropertyChanged(); }
        }
        private string speedText;

        /// <summary>下载是否正在进行中。</summary>
        public bool IsActive {
            get => isActive;
            set { isActive = value; OnPropertyChanged(); }
        }
        private bool isActive;

        /// <summary>进度条可见性。</summary>
        public Visibility Visibility {
            get => visibility;
            set { visibility = value; OnPropertyChanged(); }
        }
        private Visibility visibility = Visibility.Collapsed;


        // ===================== INotifyPropertyChanged =====================
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
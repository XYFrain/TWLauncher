using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace TWLauncher {
    internal class ProgressViewModel : INotifyPropertyChanged {
        public static ProgressViewModel Instance { get; } = new ProgressViewModel();

        // ===================== 构造函数 =====================
        private ProgressViewModel() { }

        // ===================== 属性（XAML 绑定） =====================
        public Visibility Visibility {
            get => visibility;
            set { visibility = value; OnPropertyChanged(); }
        }
        private Visibility visibility = Visibility.Collapsed;
        public double Percent {
            get => percent;
            set { percent = value; OnPropertyChanged(); }
        }
        private double percent;
        public string StageText {
            get => stageText;
            set { stageText = value; OnPropertyChanged(); }
        }
        private string stageText;
        public string SpeedText {
            get => speedText;
            set { speedText = value; OnPropertyChanged(); }
        }
        private string speedText;
        public bool IsActive {
            get => isActive;
            set { isActive = value; OnPropertyChanged(); }
        }
        private bool isActive;

        // ===================== 命令（XAML 绑定） =====================

        // ===================== 方法 =====================

        // ===================== INotifyPropertyChanged =====================
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
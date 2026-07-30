using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using TWLauncher.Controller;

namespace TWLauncher {
    /// <summary>
    /// 主窗口 ViewModel（单例）：暴露子 ViewModel 和命令供 XAML 绑定。
    /// 流程逻辑委托给 MainButtonController。
    /// </summary>
    internal class MainViewModel : INotifyPropertyChanged {
        public static MainViewModel Instance { get; } = new MainViewModel();

        private MainViewModel() {
            GameActionCommand = new RelayCommand(MainButtonController.HandleMainButton);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            CloseSettingsCommand = new RelayCommand(CloseSettings);
        }

        // ===================== 子 ViewModel（XAML 绑定） =====================

        public MainButtonViewModel MainButtonVM => MainButtonViewModel.Instance;
        public ProgressViewModel ProgressVM => ProgressViewModel.Instance;
        public SettingsViewModel SettingsVM => SettingsViewModel.Instance;

        // ===================== 命令（XAML 绑定） =====================

        public ICommand GameActionCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand CloseSettingsCommand { get; }

        // ===================== 设置面板 =====================

        private void OpenSettings() {
            SettingsViewModel.Instance.Visibility = Visibility.Visible;
        }
        private void CloseSettings() {
            SettingsViewModel.Instance.Visibility = Visibility.Collapsed;
        }

        // ===================== INotifyPropertyChanged =====================
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
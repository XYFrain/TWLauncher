using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using TWLauncher.Controller;

namespace TWLauncher {
    internal class MainViewModel : INotifyPropertyChanged {
        public static MainViewModel Instance { get; } = new MainViewModel();

        // ===================== 构造函数 =====================
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

        // ===================== 方法 =====================
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
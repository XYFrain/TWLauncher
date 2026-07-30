using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace TWLauncher {
    /// <summary>
    /// 设置面板 ViewModel（单例）。
    /// </summary>
    internal class SettingsViewModel : INotifyPropertyChanged {

        public static SettingsViewModel Instance { get; } = new SettingsViewModel();
        private SettingsViewModel() { }

        /// <summary>设置面板可见性。</summary>
        public Visibility Visibility {
            get => visibility;
            set { visibility = value; OnPropertyChanged(); }
        }
        private Visibility visibility = Visibility.Collapsed;



        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
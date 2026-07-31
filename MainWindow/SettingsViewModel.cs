using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using TWLauncher.Controller;
using TWLauncher.Models;
using TWLauncher.Utils;

namespace TWLauncher {
    internal class SettingsViewModel : INotifyPropertyChanged {
        public static SettingsViewModel Instance { get; } = new SettingsViewModel();

        // ===================== 构造函数 =====================
        private SettingsViewModel() {
            BrowseJavaCommand = new RelayCommand(BrowseJava);
        }

        // ===================== 属性（XAML 绑定） =====================
        public Visibility Visibility {
            get => visibility;
            set {
                visibility = value;
                OnPropertyChanged();
            }
        }
        private Visibility visibility = Visibility.Collapsed;
        public string MaxMemoryText {
            get { return ConfigController.MaxMemory.ToString(); }
            set {
                int val;
                if (int.TryParse(value, out val) && val > 0) {
                    ConfigController.MaxMemory = val;
                }
                OnPropertyChanged(nameof(MaxMemoryText));
            }
        }
        public string JavaPath {
            get { return ConfigController.JavaPath; }
            set {
                ConfigController.JavaPath = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<JavaPath> JavaPathList => ConfigController.JavaPathList;

        // ===================== 命令（XAML 绑定） =====================
        public ICommand BrowseJavaCommand { get; }

        // ===================== 方法 =====================
        private void BrowseJava() {
            var validated = JavaUtil.PickJava();
            if (validated != null) {
                var list = ConfigController.JavaPathList;
                bool exists = false;
                foreach (var entry in list) {
                    if (string.Equals(entry.Path, validated.Path, StringComparison.OrdinalIgnoreCase)) {
                        exists = true;
                        break;
                    }
                }
                if (!exists)
                    list.Add(validated);
                ConfigController.JavaPath = validated.Path;
                OnPropertyChanged(nameof(JavaPath));
            }
        }

        // ===================== INotifyPropertyChanged =====================

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

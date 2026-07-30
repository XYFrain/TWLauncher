using System;
using System.Windows.Input;

namespace TWLauncher {
    /// <summary>
    /// ICommand 的轻量实现，把方法包成 WPF 命令供 XAML 绑定。
    /// 你只管传一个 Action，按钮点击时就调用它。
    /// </summary>
    internal class RelayCommand : ICommand {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null) {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _execute();
    }
}

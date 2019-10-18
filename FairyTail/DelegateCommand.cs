using System;
using System.Windows.Input;

namespace FTail
{
    public class DelegateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        readonly Action callback;

        public DelegateCommand(Action callback) => this.callback = callback;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => callback();
    }
}

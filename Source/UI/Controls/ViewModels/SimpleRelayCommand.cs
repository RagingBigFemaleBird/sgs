using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Sanguosha.UI.Controls
{
    public class SimpleRelayCommand : ViewModelBase, ICommand
    {
        #region Fields

        readonly Action<object> _execute;

        public Action<object> Executor
        {
            get { return _execute; }
        } 


        private bool _canExecute;

        public bool CanExecuteStatus 
        { 
            get {return _canExecute;}
            set
            {
                if (_canExecute == value) return;
                _canExecute = value;
                var handle = CanExecuteChanged;
                if (handle != null)
                {
                    handle(this, new EventArgs());
                }
            }
        }

        #endregion // Fields

        #region Constructors

        public SimpleRelayCommand(Action<object> execute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
        }

        #endregion // Constructors

        #region ICommand Members

        public virtual bool CanExecute(object parameter)
        {
            return CanExecuteStatus;
        }

        public event EventHandler CanExecuteChanged;

        public virtual void Execute(object parameter)
        {
            _execute(parameter);
        }

        #endregion // ICommand Members
    }
}

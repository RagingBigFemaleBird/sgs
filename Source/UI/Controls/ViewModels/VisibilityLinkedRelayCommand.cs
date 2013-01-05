using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Sanguosha.UI.Controls
{
    public class VisibilityLinkedRelayCommand : SimpleRelayCommand
    {
        public VisibilityLinkedRelayCommand(Action<object> execute) : base(execute)
        {
        }

        #region Fields

        bool _isVisible;

        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                if (_isVisible == value) return;
                _isVisible = value;
                OnPropertyChanged("IsVisible");
            }
        }

        #endregion // Fields

    }
}

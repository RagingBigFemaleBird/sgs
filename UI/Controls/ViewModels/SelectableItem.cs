using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.UI.Controls
{
    public class SelectableItem : ViewModelBase
    {
        #region UI Related Properties

        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected == value || !IsSelectionMode || !IsEnabled)
                {
                    return;
                }
                bool oldValue = isSelected;
                EventHandler handle = OnSelectedChanged;
                if (handle == null) return;
                isSelected = value;
                _EnsureSelectionInvariant();
                if (oldValue != isSelected)
                {
                    OnPropertyChanged("IsSelected");
                }
                handle(this, new EventArgs());
            }
        }

        private void _EnsureSelectionInvariant()
        {
            if (!IsSelectionMode)
            {
                if (isEnabled)
                {
                    isEnabled = false;
                    OnPropertyChanged("IsEnabled");
                }
                if (isSelected)
                {
                    isSelected = false;
                    OnPropertyChanged("IsSelected");
                }
                if (isFaded)
                {
                    isFaded = false;
                    OnPropertyChanged("IsFaded");
                }
            }
            else if (!IsEnabled)
            {
                if (isSelected)
                {
                    isSelected = false;
                    OnPropertyChanged("IsSelected");
                }
                if (!isFaded)
                {
                    isFaded = true;
                    OnPropertyChanged("IsFaded");
                }
            }
            else
            {
                if (isFaded)
                {
                    isFaded = false;
                    OnPropertyChanged("IsFaded");
                }
            }
        }

        private bool isSelectionMode;

        public bool IsSelectionMode
        {
            get { return isSelectionMode; }
            set
            {
                if (isSelectionMode == value) return;
                isSelectionMode = value;
                if (value == true)
                {
                    IsEnabled = true;
                }
                else
                {
                    _EnsureSelectionInvariant();
                }
                OnPropertyChanged("IsSelectionMode");
            }
        }

        private bool isEnabled;

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (isEnabled == value) return;
                bool oldValue = IsEnabled;
                isEnabled = value;
                _EnsureSelectionInvariant();
                if (oldValue != IsEnabled)
                {
                    OnPropertyChanged("IsEnabled");
                }
            }
        }

        private bool isFaded;

        public bool IsFaded
        {
            get { return isFaded; }
            set
            {
                if (isFaded == value) return;
                isFaded = value;
                OnPropertyChanged("IsFaded");
            }
        }

        public event EventHandler OnSelectedChanged;

        #endregion
    }
}

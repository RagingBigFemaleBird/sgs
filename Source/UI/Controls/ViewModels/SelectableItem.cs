using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.UI.Controls
{
    public class SelectableItem : ViewModelBase
    {

        #region Constructors
        public SelectableItem()
        {            
        }
        #endregion

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
                isSelected = value;
                _EnsureSelectionInvariant();
                if (oldValue != isSelected)
                {
                    OnPropertyChanged("IsSelected");
                    if (isSelected && SelectedTimes == 0)
                    {
                        SelectedTimes = 1;
                    }
                    else if (!isSelected)
                    {
                        SelectedTimes = 0;
                    }
                    EventHandler handle = OnSelectedChanged;
                    if (handle != null)
                    {
                        handle(this, new EventArgs());
                    }
                }
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
                    SelectedTimes = 0;
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
                    SelectedTimes = 0;
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

        private bool _isSelectionRepeatable;
        public bool IsSelectionRepeatable 
        {
            get
            {
                return _isSelectionRepeatable;
            }
            set
            {
                if (_isSelectionRepeatable == value) return;
                _isSelectionRepeatable = value;
                OnPropertyChanged("IsSelectionRepeatable");
            }
        }

        private int _selectedTimes;
        public int SelectedTimes 
        {
            get
            {
                return _selectedTimes;
            }
            set
            {
                if (_selectedTimes == value) return;
                _selectedTimes = value;
                OnPropertyChanged("SelectedTimes");
            }
        }

        public bool CanBeSelectedMore { get; set; }

        public event EventHandler OnSelectedChanged;

        #endregion

        public void SelectOnce()
        {
            if (IsSelectionRepeatable && IsSelected)
            {
                if (!CanBeSelectedMore)
                {
                    SelectedTimes = 0;
                    IsSelected = false;
                }
                else
                {
                    SelectedTimes++;
                    EventHandler handle = OnSelectedChanged;
                    if (handle != null)
                    {
                        handle(this, new EventArgs());
                    }
                }
            }
            else
            {
                IsSelected = !IsSelected;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Games;

namespace Sanguosha.UI.Controls
{
    public class RoleIconViewModel : ViewModelBase
    {
        Role _role;

        public Role Role
        {
            get { return _role; }
            set 
            {
                if (_role == value) return;
                _role = value;
                RoleString = string.Concat(Role.ToString(), ".", _isAlive ? "Alive" : "Dead");
            }
        }

        bool _isAlive;

        public bool IsAlive
        {
            get { return _isAlive; }
            set 
            {
                if (_isAlive == value) return;
                _isAlive = value;
                RoleString = string.Concat(Role.ToString(), ".", _isAlive ? "Alive" : "Dead");
            }
        }

        private string _roleString;

        public string RoleString
        {
            get { return _roleString; }
            private set 
            {
                if (_roleString == value) return;
                _roleString = value;
                OnPropertyChanged("RoleString");
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.UI.Controls
{
    public class CardSlotViewModel : CardViewModel
    {
        private string hint;
        public string Hint
        {
            get
            {
                return hint;
            }
            set
            {
                if (hint == value) return;
                hint = value;
                OnPropertyChanged("Hint");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Players;
using System.Collections.ObjectModel;

namespace Sanguosha.UI.Controls
{

    public enum GameResult
    {
        Win,
        Lose,
        Draw,
    }

    public class GameResultViewModel : ViewModelBase
    {
        public Player Player { get; set; }
        public string UserName { get; set; }
        public string GainedExperience { get; set; }
        public string GainedTechPoints { get; set; }
        public GameResult Result { get; set; }
        public string Comments { get; set; }
    }
}

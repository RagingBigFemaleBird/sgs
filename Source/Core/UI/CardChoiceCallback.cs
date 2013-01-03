using Sanguosha.Core.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.UI
{
    public class CardChoiceCallback
    {
        public static void GenericCardChoiceCallback(CardRearrangement obj)
        {
            if (Game.CurrentGame.IsClient)
            {
                Game.CurrentGame.GameClient.CardChoiceCallBack(obj);
            }
        }
    }
}

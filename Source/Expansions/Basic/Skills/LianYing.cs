using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Basic.Skills
{
    /// <summary>
    /// 连营-当你失去最后一张手牌时，你可以摸一张牌。
    /// </summary>
    public class LianYing : PassiveSkill
    {
        class LianYingTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source == null || eventArgs.Source != Owner || Game.CurrentGame.Decks[Owner, DeckType.Hand].Count > 0)
                {
                    return;
                }
                int answer = 0;
                if (Game.CurrentGame.UiProxies[Owner].AskForMultipleChoice(new MultipleChoicePrompt("LianYing"), Prompt.YesNoChoices, out answer) && answer == 0)
                {
                    Game.CurrentGame.DrawCards(Owner, 1);
                }
            }

            public LianYingTrigger(Player p)
            {
                Owner = p;
            }
        }

        protected override void InstallTriggers(Sanguosha.Core.Players.Player owner)
        {
            Game.CurrentGame.RegisterTrigger(GameEvent.CardsLost, new LianYingTrigger(owner));
        }
    }
}

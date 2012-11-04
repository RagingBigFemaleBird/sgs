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
    /// 闭月-回合结束阶段开始时，你可以摸一张牌。
    /// </summary>
    public class BiYue : PassiveSkill
    {
        class BiYueTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                {
                    return;
                }
                int answer = 0;
                if (Game.CurrentGame.UiProxies[Owner].AskForMultipleChoice(new MultipleChoicePrompt("BiYue"), Prompt.YesNoChoices, out answer) && answer == 0)
                {
                    Game.CurrentGame.DrawCards(Owner, 1);
                }
            }

            public BiYueTrigger(Player p)
            {
                Owner = p;
            }
        }

        Trigger theTrigger;

        protected override void InstallTriggers(Sanguosha.Core.Players.Player owner)
        {
            theTrigger = new BiYueTrigger(owner);
            Game.CurrentGame.RegisterTrigger(GameEvent.PhaseBeginEvents[TurnPhase.End], theTrigger);
        }

        protected override void UninstallTriggers(Player owner)
        {
            if (theTrigger != null)
            {
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhaseBeginEvents[TurnPhase.End], theTrigger);
            }
        }

    }
}

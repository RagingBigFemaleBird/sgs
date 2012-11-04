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
    /// 英姿-摸牌阶段摸牌时，你可以额外摸一张牌。
    /// </summary>
    public class YingZi : TriggerSkill
    {
        class YingZiTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                {
                    return;
                }
                int answer = 0;
                if (Game.CurrentGame.UiProxies[Owner].AskForMultipleChoice(new MultipleChoicePrompt("YingZi"), Prompt.YesNoChoices, out answer) && answer == 0)
                {
                    Owner[Player.DealAdjustment]++;
                }
                return;
            }
        }

        public YingZi()
        {
            Triggers.Add(GameEvent.PhaseProceedEvents[TurnPhase.Draw], new YingZiTrigger());
        }

    }
}
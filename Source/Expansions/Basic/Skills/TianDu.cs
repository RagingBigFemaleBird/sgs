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
    /// 天妒-在你的判定牌生效后，你可以获得此牌。
    /// </summary>
    public class TianDu : PassiveSkill
    {
        class TianDuTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                {
                    return;
                }
                //someone already took it...
                if (Game.CurrentGame.Decks[eventArgs.Source, DeckType.JudgeResult].Count == 0)
                {
                    return;
                }
                int answer = 0;
                if (Game.CurrentGame.UiProxies[Owner].AskForMultipleChoice(new MultipleChoicePrompt("TianDu"), Prompt.YesNoChoices, out answer) && answer == 0)
                {
                    Game.CurrentGame.HandleCardTransferToHand(Owner, Owner, new List<Card>(Game.CurrentGame.Decks[eventArgs.Source, DeckType.JudgeResult]));
                }
                return;
            }
            public TianDuTrigger(Player p)
            {
                Owner = p;
            }
        }

        Trigger theTrigger;

        protected override void InstallTriggers(Sanguosha.Core.Players.Player owner)
        {
            theTrigger = new TianDuTrigger(owner);
            Game.CurrentGame.RegisterTrigger(GameEvent.PlayerJudgeDone, theTrigger);
        }

        protected override void UninstallTriggers(Player owner)
        {
            if (theTrigger != null)
            {
                Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerJudgeDone, theTrigger);
            }
        }

    }
}

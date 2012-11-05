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
    /// 洛神-回合开始阶段开始时，你可以进行一次判定：若结果为黑色（通常是完全不可能的），你获得此牌；你可以重复此流程，直到出现红色的判定结果为止。
    /// </summary>
    public class LuoShen : TriggerSkill
    {
        class LuoShenJudgeTrigger : Trigger
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
                if (eventArgs.Card.SuitColor == SuitColorType.Black)
                {
                    Game.CurrentGame.HandleCardTransferToHand(Owner, Owner, new List<Card>(Game.CurrentGame.Decks[eventArgs.Source, DeckType.JudgeResult]));
                }
                else
                {
                    Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerJudgeDone, this);
                }
                return;
            }
            public LuoShenJudgeTrigger(Player p)
            {
                Owner = p;
            }
        }

        void OnPhaseBegin(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            int answer = 0;
            if (Game.CurrentGame.UiProxies[Owner].AskForMultipleChoice(new MultipleChoicePrompt("LuoShen"), Prompt.YesNoChoices, out answer) && answer == 0)
            {
                Game.CurrentGame.RegisterTrigger(GameEvent.PlayerJudgeDone, new LuoShenJudgeTrigger(Owner));
                ReadOnlyCard c;
                do
                {
                    c = Game.CurrentGame.Judge(Owner);
                } while (c.SuitColor == SuitColorType.Black && Game.CurrentGame.UiProxies[Owner].AskForMultipleChoice(new MultipleChoicePrompt("LuoShen"), Prompt.YesNoChoices, out answer) && answer == 0);
            }
        }

        public LuoShen()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return true; },
                OnPhaseBegin,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false };
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Start], trigger);
        }
    }
}

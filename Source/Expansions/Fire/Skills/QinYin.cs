using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Fire.Skills
{
    /// <summary>
    /// 琴音-弃牌阶段，当你弃置了两张或更多的手牌时，你可令所有角色各回复1点体力或各失去1点体力。
    /// </summary>
    public class QinYin : TriggerSkill
    {
        public static PlayerAttribute QinYinUsable = PlayerAttribute.Register("QinYinUsable", true);
        public static PlayerAttribute QinYinUsed = PlayerAttribute.Register("QinYinUsed", true);


        public void Run(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (eventArgs.Cards != null)
            {
                owner[QinYinUsable] += eventArgs.Cards.Count;
            }
            if (owner[QinYinUsable] >= 2)
            {
                List<string> QinYinQuestion = new List<string>();
                QinYinQuestion.Add(Prompt.MultipleChoiceOptionPrefix + "QinYinHuiFu");
                QinYinQuestion.Add(Prompt.MultipleChoiceOptionPrefix + "QinYinShiQu");
                QinYinQuestion.Add(Prompt.NoChoice);
                int answer;
                if (Game.CurrentGame.UiProxies[owner].AskForMultipleChoice(new MultipleChoicePrompt("QinYin"), QinYinQuestion, out answer))
                {
                    if (answer == 2)
                        return;
                    owner[QinYinUsed] = 1;
                    NotifySkillUse(new List<Player>());
                    if (answer == 0)
                    {
                        foreach (var p in Game.CurrentGame.AlivePlayers)
                        {
                            Game.CurrentGame.RecoverHealth(owner, p, 1);
                        }
                    }
                    else if (answer == 1)
                    {
                        foreach (var p in Game.CurrentGame.AlivePlayers)
                        {
                            Game.CurrentGame.LoseHealth(p, 1);
                        }
                    }
                }
            }
        }
        

        public QinYin()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Game.CurrentGame.CurrentPhase == TurnPhase.Discard && Game.CurrentGame.CurrentPlayer == p && p[QinYinUsed] == 0;},
                Run,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };

            Triggers.Add(GameEvent.CardsEnteredDiscardDeck, trigger);
            IsAutoInvoked = null;
        }
    }
}

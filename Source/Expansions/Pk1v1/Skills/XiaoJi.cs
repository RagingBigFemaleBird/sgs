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

namespace Sanguosha.Expansions.Pk1v1.Skills
{
    /// <summary>
    /// 枭姬-每当你失去一张装备区里的牌时，你可以选择一项：摸两张牌，回复1点体力。
    /// </summary>
    public class XiaoJi2 : TriggerSkill
    {
        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            foreach (Card c in eventArgs.Cards)
            {
                if (c.HistoryPlace1.DeckType == DeckType.Equipment && c.HistoryPlace1.Player == Owner)
                {
                    if (Owner.LostHealth == 0)
                    {
                        if (AskForSkillUse())
                        {
                            Game.CurrentGame.DrawCards(Owner, 2);
                        }
                    }
                    else
                    {
                        int answer;
                        if (Owner.AskForMultipleChoice(new MultipleChoicePrompt("XiaoJi"), new List<OptionPrompt>() { OptionPrompt.NoChoice, new OptionPrompt("MoPai"), new OptionPrompt("HuiFu") }, out answer))
                        {
                            NotifySkillUse(new List<Player>());
                            if (answer == 1)
                            {
                                Game.CurrentGame.DrawCards(Owner, 2);
                            }
                            if (answer == 2)
                            {
                                Game.CurrentGame.RecoverHealth(Owner, Owner, 1);
                            }
                        }
                    }
                }
            }
        }

        public XiaoJi2()
        {
            var trigger = new RelayTrigger(
                Run,
                TriggerCondition.OwnerIsSource
            ) { };
            Triggers.Add(GameEvent.CardsLost, trigger);
            IsAutoInvoked = null;
        }
    }
}

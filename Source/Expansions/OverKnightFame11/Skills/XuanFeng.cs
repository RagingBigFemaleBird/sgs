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
using Sanguosha.Expansions.Basic.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.OverKnightFame11.Skills
{
    /// <summary>
    /// 旋风-当你失去装备区里的牌时，或于弃牌阶段内弃置了两张或更多的手牌后，你可以依次弃置一至两名其他角色的共计两张牌。
    /// </summary>
    public class XuanFeng : TriggerSkill
    {
        private PlayerAttribute XuanFengUsable = PlayerAttribute.Register("XuanFengUsable", true);
        private PlayerAttribute XuanFengUsed = PlayerAttribute.Register("XuanFengUsed", true);

        bool canTrigger(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (gameEvent == GameEvent.CardsLost)
            {
                foreach (Card card in eventArgs.Cards)
                {
                    if (card.HistoryPlace1.Player == Owner && card.HistoryPlace1.DeckType == DeckType.Equipment)
                    {
                        return true;
                    }
                }
                return false;
            }
            else if (gameEvent == GameEvent.CardsEnteredDiscardDeck)
            {
                if (!(Game.CurrentGame.PhasesOwner == Owner && Game.CurrentGame.CurrentPhase == TurnPhase.Discard && Owner[XuanFengUsed] == 0))
                {
                    return false;
                }
                if (eventArgs.Cards != null)
                {
                    Owner[XuanFengUsable] += eventArgs.Cards.Count;
                }
                return Owner[XuanFengUsable] >= 2;
            }
            return true;
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs, List<Card> cards, List<Player> players)
        {
            int count = 2 / players.Count;
            List<Player> tempPlayers = new List<Player>(players);
            Game.CurrentGame.SortByOrderOfComputation(Game.CurrentGame.CurrentPlayer, tempPlayers);
            foreach (Player target in tempPlayers)
            {
                for (int i = 0; i < count; i++)
                {
                    if (target.Equipments().Count + target.HandCards().Count == 0) break;
                    Card theCard = Game.CurrentGame.SelectACardFrom(target, Owner, new CardChoicePrompt("XuanFeng", target, Owner), "QiPaiDui");
                    Game.CurrentGame.HandleCardDiscard(target, new List<Card>() { theCard });
                }
            }
        }

        class XuanFengVerifier : CardsAndTargetsVerifier
        {
            public XuanFengVerifier()
            {
                MaxPlayers = 2;
                MinPlayers = 1;
                MaxCards = 0;
            }

            protected override bool VerifyPlayer(Player source, Player player)
            {
                return player != source && player.HandCards().Count + player.Equipments().Count > 0;
            }
        }

        public XuanFeng()
        {
            var trigger = new AutoNotifyUsagePassiveSkillTrigger(
                this,
                canTrigger,
                Run,
                TriggerCondition.OwnerIsSource,
                new XuanFengVerifier()
            ) { Priority = SkillPriority.XiaoJi };
            Triggers.Add(GameEvent.CardsLost, trigger);
            Triggers.Add(GameEvent.CardsEnteredDiscardDeck, trigger);
            IsAutoInvoked = null;
        }
    }
}

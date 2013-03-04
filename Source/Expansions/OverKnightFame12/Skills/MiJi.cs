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

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    /// <summary>
    /// 秘计-回合结束阶段开始时，若你已受伤，你可以摸至多X张牌（X为你已损失的体力值），然后将相同数量的手牌以任意方式交给任意数量的其他角色。
    /// </summary>
    public class MiJi : TriggerSkill
    {
        class MiJiVerifier : CardsAndTargetsVerifier
        {
            public MiJiVerifier(int count)
            {
                MaxPlayers = 1;
                MinPlayers = 1;
                MaxCards = count;
                MinCards = 1;
                Helper.NoCardReveal = true;
            }

            protected override bool VerifyPlayer(Player source, Player player)
            {
                return source != player;
            }

            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Place.DeckType == DeckType.Hand;
            }
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            int drawCount = Owner.LostHealth;
            Game.CurrentGame.DrawCards(Owner, drawCount);
            drawCount = Math.Min(Owner.HandCards().Count, drawCount);
            while (drawCount > 0)
            {
                ISkill skill;
                List<Card> cards;
                List<Player> players;
                if (Owner.AskForCardUsage(new CardUsagePrompt("MiJi", drawCount), new MiJiVerifier(drawCount), out skill, out cards, out players))
                {
                    drawCount -= cards.Count;
                    Game.CurrentGame.HandleCardTransferToHand(Owner, players[0], cards);
                }
                else
                {
                    drawCount = 0;
                }
            }
        }

        public MiJi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                   this,
                   (p, e, a) => { return p.LostHealth > 0; },
                   Run,
                   TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger);
        }
    }
}

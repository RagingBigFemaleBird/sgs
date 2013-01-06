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
    public class MiJi : TriggerSkill
    {
        /// <summary>
        /// 秘计-回合开始/结束阶段开始时，若你已受伤，你可以进行一次判定，若判定结果为黑色，你观看牌堆顶的X张牌（X为你已损失的体力值），然后将这些牌交给一名角色。
        /// </summary>
        class MiJiVerifier : CardsAndTargetsVerifier
        {
            public MiJiVerifier()
            {
                MaxPlayers = 1;
                MinPlayers = 1;
                MaxCards = 0;
                MinCards = 0;
                Helper.NoCardReveal = true;
            }
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            var card = Game.CurrentGame.Judge(Owner, this, null, (judgeResultCard) => { return judgeResultCard.SuitColor == SuitColorType.Black; });
            if (card.SuitColor == SuitColorType.Black)
            {
                ISkill skill;
                List<Card> cards;
                List<Player> players;
                int toDraw = Owner.LostHealth;
                List<Card> remainingCards = new List<Card>();
                CardsMovement move = new CardsMovement();
                for (int i = 0; i < toDraw; i++)
                {
                    Game.CurrentGame.SyncImmutableCard(Owner, Game.CurrentGame.PeekCard(0));
                    Card c = Game.CurrentGame.DrawCard();
                    move.Cards.Add(c);
                    remainingCards.Add(c);
                }
                move.To = new DeckPlace(Owner, DeckType.Hand);
                move.Helper.IsFakedMove = true;
                Game.CurrentGame.MoveCards(move);
                if (!Owner.AskForCardUsage(new CardUsagePrompt("MiJi"), new MiJiVerifier(), out skill, out cards, out players))
                {
                    players = new List<Player>() { Owner };
                }
                Game.CurrentGame.InsertBeforeDeal(null, remainingCards, new MovementHelper() { IsFakedMove = true });
                Game.CurrentGame.DrawCards(players[0], toDraw);
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
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Start], trigger);
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger);
        }
    }
}

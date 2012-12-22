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

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 再起-摸牌阶段，若你已受伤，你可以放弃摸牌改为展示牌堆顶的X张牌(X为你已损失的体力值)，其中每有一张红桃牌，你回复1点体力，然后弃置这些红桃牌，并获得其余的牌。
    /// </summary>
    public class ZaiQi : TriggerSkill
    {
        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            DeckType HuoShouDeck = new DeckType("HuoShou");

            CardsMovement move = new CardsMovement();
            move.Cards = new List<Card>();
            int toDraw = Owner.MaxHealth - Owner.Health;
            for (int i = 0; i < toDraw; i++)
            {
                Game.CurrentGame.SyncImmutableCardAll(Game.CurrentGame.PeekCard(0));
                Card c = Game.CurrentGame.DrawCard();
                move.Cards.Add(c);
            }
            move.To = new DeckPlace(null, HuoShouDeck);
            Game.CurrentGame.MoveCards(move);
            List<Card> toDiscard = new List<Card>();
            foreach (var c in move.Cards)
            {
                if (c.Suit == SuitType.Heart)
                {
                    toDiscard.Add(c);
                    Game.CurrentGame.RecoverHealth(Owner, Owner, 1);
                }
            }
            Game.CurrentGame.HandleCardDiscard(null, toDiscard);
            Game.CurrentGame.HandleCardTransferToHand(null, Owner, Game.CurrentGame.Decks[null, HuoShouDeck]);
            Game.CurrentGame.CurrentPhaseEventIndex++;
            throw new TriggerResultException(TriggerResult.End);

        }


        public ZaiQi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return p.MaxHealth > p.Health; },
                Run,
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Draw], trigger);
            IsAutoInvoked = false;
        }

    }
}

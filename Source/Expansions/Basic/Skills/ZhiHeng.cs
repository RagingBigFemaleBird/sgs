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

namespace Sanguosha.Expansions.Basic.Skills
{
    /// <summary>
    /// 制衡-出牌阶段，你可以弃置任意数量的牌，然后摸等量的牌，每阶段限一次。 
    /// </summary>
    public class ZhiHeng : ActiveSkill
    {
        public override VerifierResult Validate(GameEventArgs arg)
        {
            if (Owner[ZhiHengUsed] != 0)
            {
                return VerifierResult.Fail;
            }
            List<Card> cards = arg.Cards;
            if (arg.Targets != null && arg.Targets.Count != 0)
            {
                return VerifierResult.Fail;
            }
            if (cards == null || cards.Count == 0)
            {
                return VerifierResult.Partial;
            }
            foreach (Card card in cards)
            {
                if (card.Owner != Owner || card.Place.DeckType != DeckType.Hand)
                {
                    return VerifierResult.Fail;
                }
            }
            return VerifierResult.Success;
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[ZhiHengUsed] = 1;
            List<Card> cards = arg.Cards;
            Trace.Assert(cards.Count > 0);
            CardsMovement move = new CardsMovement();
            move.cards = new List<Card>(cards);
            move.to = new DeckPlace(null, DeckType.Discard);
            int toDraw = cards.Count;
            Game.CurrentGame.MoveCards(move, null);
            Game.CurrentGame.DrawCards(Owner, toDraw);
            return true;
        }

        public override Core.Players.Player Owner
        {
            get
            {
                return base.Owner;
            }
            set
            {
                base.Owner = value;
                Owner.AutoResetAttributes.Add(ZhiHengUsed);
            }
        }

        public static readonly string ZhiHengUsed = "ZhiHengUsed";

        public override void CardRevealPolicy(Core.Players.Player p, List<Card> cards, List<Core.Players.Player> players)
        {
            foreach (Card c in cards)
            {
                c.RevealOnce = true;
            }
        }
    }
}

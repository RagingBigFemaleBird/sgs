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
                if (card.Owner != Owner)
                {
                    return VerifierResult.Fail;
                }
                if (!Game.CurrentGame.PlayerCanDiscardCard(Owner, card))
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
            int toDraw = cards.Count;
            Game.CurrentGame.HandleCardDiscard(Owner, cards);
            Game.CurrentGame.DrawCards(Owner, toDraw);
            return true;
        }

        public static PlayerAttribute ZhiHengUsed = PlayerAttribute.Register("ZhiHengUsed", true);

        public override void CardRevealPolicy(Core.Players.Player p, List<Card> cards, List<Core.Players.Player> players)
        {
            foreach (Card c in cards)
            {
                c.RevealOnce = true;
            }
        }
    }
}

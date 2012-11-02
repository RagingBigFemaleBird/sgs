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
    /// 青囊―出牌阶段，你可以弃置一张手牌，令一名已受伤的角色回复1点体力。每阶段限一次。
    /// </summary>
    public class QingNang : ActiveSkill
    {
        public override VerifierResult Validate(GameEventArgs arg)
        {
            if (Owner[QingNangUsed] != 0)
            {
                return VerifierResult.Fail;
            }
            List<Card> cards = arg.Cards;
            if ((cards == null || cards.Count == 0) && (arg.Targets == null || arg.Targets.Count == 0))
            {
                return VerifierResult.Partial;
            }
            if (cards != null && cards.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets != null && arg.Targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            foreach (Card card in cards)
            {
                if (card.Owner != Owner || card.Place.DeckType != DeckType.Hand)
                {
                    return VerifierResult.Fail;
                }
                if (!Game.CurrentGame.PlayerCanDiscardCard(Owner, card))
                {
                    return VerifierResult.Fail;
                }
            }
            if (arg.Targets != null && arg.Targets.Count == 1 && arg.Targets[0].Health >= arg.Targets[0].MaxHealth)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets == null || arg.Targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (arg.Cards == null || arg.Cards.Count == 0)
            {
                return VerifierResult.Partial;
            }
            return VerifierResult.Success;
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[QingNangUsed] = 1;
            List<Card> cards = arg.Cards;
            Trace.Assert(cards.Count == 1 && arg.Targets.Count == 1);
            Game.CurrentGame.HandleCardDiscard(Owner, cards);
            Game.CurrentGame.RecoverHealth(Owner, arg.Targets[0], 1);
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
                Owner.AddAutoResetAttribute(QingNangUsed);
            }
        }

        private static readonly string QingNangUsed = "QingNangUsed";

        public override void CardRevealPolicy(Core.Players.Player p, List<Card> cards, List<Core.Players.Player> players)
        {
            foreach (Card c in cards)
            {
                c.RevealOnce = true;
            }
        }
    }
}

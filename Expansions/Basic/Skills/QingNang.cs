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
        public override VerifierResult Validate(List<Card> cards, GameEventArgs arg)
        {
            if (cards == null || cards.Count == 0 || arg.Targets == null || arg.Targets.Count == 0)
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
            if (arg.Targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets[0] == Owner)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public override bool Commit(List<Card> cards, GameEventArgs arg)
        {
            Trace.Assert(cards.Count > 0 && arg.Targets.Count == 1);
            CardsMovement move;
            move.cards = new List<Card>(cards);
            move.to = new DeckPlace(null, DeckType.Discard);
            Game.CurrentGame.RecoverHealth(Owner, arg.Targets[0], 1);
            return true;
        }
    }
}

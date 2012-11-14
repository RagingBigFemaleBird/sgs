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
    /// 结姻―出牌阶段，你可以弃置两张手牌并选择一名已受伤的男性角色，你与其各回复1点体力。每阶段限一次。
    /// </summary>
    public class JieYin : ActiveSkill
    {
        public override VerifierResult Validate(GameEventArgs arg)
        {
            if (Owner[JieYinUsed] != 0)
            {
                return VerifierResult.Fail;
            }
            List<Card> cards = arg.Cards;
            if (cards == null || cards.Count <= 1)
            {
                if (arg.Targets == null || arg.Targets.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                else
                {
                    return VerifierResult.Fail;
                }
            }
            if (cards != null && cards.Count > 2)
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
            if (arg.Targets != null && arg.Targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets != null && arg.Targets.Count == 1)
            {
                if (!arg.Targets[0].IsMale)
                {
                    return VerifierResult.Fail;
                }
                if (arg.Targets[0].Health >= arg.Targets[0].MaxHealth)
                {
                    return VerifierResult.Fail;
                }
            }
            if (arg.Targets == null || arg.Targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            return VerifierResult.Success;
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[JieYinUsed] = 1;
            List<Card> cards = arg.Cards;
            Trace.Assert(cards.Count == 2 && arg.Targets.Count == 1);
            NotifyAction(arg.Source, arg.Targets, cards);
            Game.CurrentGame.HandleCardDiscard(Owner, cards);
            Game.CurrentGame.RecoverHealth(Owner, Owner, 1);
            Game.CurrentGame.RecoverHealth(Owner, arg.Targets[0], 1);
            return true;
        }

        public static PlayerAttribute JieYinUsed = PlayerAttribute.Register("JieYinUsed", true);

    }
}


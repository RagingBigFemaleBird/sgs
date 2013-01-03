using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using System.Diagnostics;

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    /// <summary>
    /// 奇策–出牌阶段，你可以将所有的手牌（至少一张）当做任意一张非延时锦囊牌使用。每阶段限一次。
    /// </summary>
    public class QiCe : CardTransformSkill, IAdditionalTypedSkill
    {
        private static PlayerAttribute QiCeUsed = PlayerAttribute.Register("QiCeUsed", true);
        public override VerifierResult TryTransform(List<Card> cards, object arg, out CompositeCard card)
        {
            card = new CompositeCard();
            card.Subcards = new List<Card>();
            card.Type = AdditionalType;
            if (Owner[QiCeUsed] == 1) return VerifierResult.Fail;
            if (Game.CurrentGame.CurrentPhase != TurnPhase.Play) return VerifierResult.Fail;
            if (Owner.HandCards().Count == 0) return VerifierResult.Fail;
            if (AdditionalType == null)
            {
                return VerifierResult.Partial;
            }
            if (!CardCategoryManager.IsCardCategory(AdditionalType.Category, CardCategory.ImmediateTool))
            {
                return VerifierResult.Fail;
            }
            if (cards != null && cards.Count > 0)
            {
                return VerifierResult.Fail;
            }

            card.Subcards.AddRange(cards);
            return VerifierResult.Success;
        }

        protected override bool DoTransformSideEffect(CompositeCard card, object arg, List<Player> targets)
        {
            Owner[QiCeUsed] = 1;
            Game.CurrentGame.SyncImmutableCardsAll(Owner.HandCards());
            card.Subcards.AddRange(Owner.HandCards());
            return true;
        }

        public CardHandler AdditionalType { get; set; }
    }
}

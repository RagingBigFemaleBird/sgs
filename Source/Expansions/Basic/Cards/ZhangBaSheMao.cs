using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;

namespace Sanguosha.Expansions.Basic.Cards
{
    public class ZhangBaSheMao : Weapon
    {
        CardTransformSkill skill;
        public ZhangBaSheMao()
        {
            skill = new ZhangBaSheMaoTransform();
        }

        public class ZhangBaSheMaoTransform : CardTransformSkill
        {
            public override VerifierResult TryTransform(List<Card> cards, object arg, out CompositeCard card)
            {
                card = null;
                if (cards == null || cards.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (cards.Count > 2)
                {
                    return VerifierResult.Fail;
                }
                if (cards[0].Owner != Owner || cards[0].Place.DeckType != DeckType.Hand)
                {
                    return VerifierResult.Fail;
                }
                if (cards.Count == 2 && (cards[1].Owner != Owner || cards[1].Place.DeckType != DeckType.Hand))
                {
                    return VerifierResult.Fail;
                }
                if (cards.Count == 1)
                {
                    return VerifierResult.Partial;
                }
                card = new CompositeCard();
                card.Subcards = new List<Card>(cards);
                card.Type = new Sha();
                return VerifierResult.Success;
            }

            public override CardHandler PossibleResult
            {
                get { return null; }
            }
        }

        public override int AttackRange
        {
            get { return 3; }
        }

        protected override void RegisterWeaponTriggers(Player p)
        {
            return;
        }

        protected override void UnregisterWeaponTriggers(Player p)
        {
            return;
        }

        public override ISkill EquipmentSkill
        {
            get
            {
                return skill;
            }
        }
    }
}

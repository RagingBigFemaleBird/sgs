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
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Battle.Cards
{

    public class ZhuQueYuShan : Weapon
    {
        ZhuQueYuShanShaSkill theTrigger;
        public ZhuQueYuShan()
        {
            theTrigger = new ZhuQueYuShanShaSkill();
            EquipmentSkill = new ZhuQueYuShanTransform();
        }

        public class ZhuQueYuShanShaSkill : TriggerSkill
        {
            public ZhuQueYuShanShaSkill()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) =>
                    {
                        return a.ReadonlyCard.Type is RegularSha;
                    },
                    (p, e, a) =>
                    {
                        a.Card.Type = new HuoSha(); a.ReadonlyCard = new ReadOnlyCard(a.Card);
                    },
                    TriggerCondition.OwnerIsSource
                ) { Priority = int.MaxValue };
                Triggers.Add(GameEvent.PlayerUsedCard, trigger);
            }
        }

        public class ZhuQueYuShanTransform : OneToOneCardTransformSkill, IEquipmentSkill
        {
            public override bool VerifyInput(Card card, object arg)
            {
                return card.Type is RegularSha;
            }

            public override CardHandler PossibleResult
            {
                get { return new HuoSha(); }
            }
        }

        public override int AttackRange
        {
            get { return 4; }
        }

        protected override void RegisterWeaponTriggers(Player p)
        {
            theTrigger.Owner = p;
            return;
        }

        protected override void UnregisterWeaponTriggers(Player p)
        {
            theTrigger.Owner = null;
            return;
        }

    }
}

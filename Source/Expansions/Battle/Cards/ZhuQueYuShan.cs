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
            theTrigger = new ZhuQueYuShanShaSkill() { ParentEquipment = this };
            EquipmentSkill = new ZhuQueYuShanTransform() { ParentEquipment = this };
        }

        public class ZhuQueYuShanShaSkill : TriggerSkill, IEquipmentSkill
        {
            public Equipment ParentEquipment { get; set; }
            public ZhuQueYuShanShaSkill()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) =>
                    {
                        if (a.Card is CompositeCard)
                        {
                            CompositeCard card = a.Card as CompositeCard;
                            return a.ReadonlyCard.Type is RegularSha && (card.Subcards == null || card.Subcards.Count == 0);
                        }
                        return a.ReadonlyCard.Type is RegularSha;
                    },
                    (p, e, a) =>
                    {
                        a.Card.Type = new HuoSha();
                        a.ReadonlyCard = new ReadOnlyCard(a.Card);
                    },
                    TriggerCondition.OwnerIsSource
                ) { Priority = int.MaxValue };
                Triggers.Add(GameEvent.PlayerUsedCard, trigger);
            }
        }

        public class ZhuQueYuShanTransform : OneToOneCardTransformSkill, IEquipmentSkill
        {
            public Equipment ParentEquipment { get; set; }
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

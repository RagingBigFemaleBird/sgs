using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;

namespace Sanguosha.Expansions.Basic.Cards
{
    
    public class ZhuGeLianNu : Weapon
    {
        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard)
        {
            throw new NotImplementedException();
        }

        public ZhuGeLianNu()
        {
            EquipmentSkill = new ZhuGeLianNuSkill();
        }

        
        class ZhuGeLianNuSkill : TriggerSkill, IEquipmentSkill
        {
            public ZhuGeLianNuSkill()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) =>
                    {
                        return a.Source[Sha.NumberOfShaUsed] > 0 &&  a.Card.Type is Sha;
                    },
                    (p, e, a) => { },
                    TriggerCondition.OwnerIsSource
                );
                var trigger2 = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { throw new TriggerResultException(TriggerResult.Success); },
                    TriggerCondition.OwnerIsSource
                ) { IsAutoNotify = false };
                var trigger3 = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) =>
                    {
                        ShaEventArgs args = (ShaEventArgs)a;
                        args.TargetApproval[0] = true;
                    },
                    TriggerCondition.OwnerIsSource
                ) { IsAutoNotify = false, Priority = int.MaxValue, Type = TriggerType.Skill };
                Triggers.Add(GameEvent.PlayerUsedCard, trigger);
                Triggers.Add(Sha.PlayerNumberOfShaCheck, trigger2);
                Triggers.Add(Sha.PlayerShaTargetValidation, trigger3);
                IsEnforced = true;
            }
        }

        public override int AttackRange
        {
            get { return 1; }
        }

        protected override void RegisterWeaponTriggers(Player p)
        {
        }

        protected override void UnregisterWeaponTriggers(Player p)
        {
        }
    }
}

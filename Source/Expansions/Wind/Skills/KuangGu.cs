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
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Wind.Skills
{
    /// <summary>
    /// 狂骨-锁定技，每当你对距离1以内的一名角色造成1点伤害后，你回复1点体力。
    /// </summary>
    public class KuangGu : TriggerSkill
    {
        class KuangGuRecover : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (Owner != eventArgs.Source || eventArgs.ReadonlyCard[KuangGuUsable] == 0)
                {
                    return;
                }
                eventArgs.ReadonlyCard[KuangGuUsable] = 0;
                if (Owner.LostHealth > 0)
                {
                    ActionLog log = new ActionLog();
                    log.GameAction = GameAction.None;
                    log.SkillAction = skill;
                    log.Source = Owner;
                    Game.CurrentGame.NotificationProxy.NotifySkillUse(log);
                    int recover = (eventArgs as DamageEventArgs).Magnitude;
                    while (recover-- > 0)
                    {
                        Game.CurrentGame.RecoverHealth(Owner, Owner, 1);
                    }
                }
                Game.CurrentGame.UnregisterTrigger(GameEvent.AfterDamageCaused, this);
            }

            KuangGu skill;
            public KuangGuRecover(Player player, KuangGu sk)
            {
                Owner = player;
                skill = sk;
            }
        }
        public KuangGu()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Game.CurrentGame.DistanceTo(p, a.Targets[0]) <= 1; },
                (p, e, a) => { a.ReadonlyCard[KuangGuUsable] = 1; Game.CurrentGame.RegisterTrigger(GameEvent.AfterDamageCaused, new KuangGuRecover(p, this)); },
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false, AskForConfirmation = false, Priority = int.MinValue };
            Triggers.Add(GameEvent.DamageInflicted, trigger);
            IsEnforced = true;
        }

        private static CardAttribute KuangGuUsable = CardAttribute.Register("KuangGuUsable");
    }
}

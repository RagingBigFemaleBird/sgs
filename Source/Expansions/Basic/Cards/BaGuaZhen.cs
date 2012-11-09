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
using System.Diagnostics;

namespace Sanguosha.Expansions.Basic.Cards
{
    public class BaGuaZhen : Armor
    {
        public class BaGuaZhenSkill : TriggerSkill
        {
            protected void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
            {
                ReadOnlyCard c = Game.CurrentGame.Judge(Owner, null, new Card() { Type = new BaGuaZhen() });
                if (c.SuitColor == SuitColorType.Red)
                {
                    eventArgs.Cards = new List<Card>();
                    ActionLog log = new ActionLog();
                    log.Source = Owner;
                    log.SkillAction = new BaGuaZhenSkill();
                    log.GameAction = GameAction.None;
                    Game.CurrentGame.NotificationProxy.NotifySkillUse(log);
                    throw new TriggerResultException(TriggerResult.Success);
                }
            }
            public BaGuaZhenSkill()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return a.Card.Type is Shan; },
                    Run,
                    TriggerCondition.OwnerIsSource
                ) { IsAutoNotify = false };
                Triggers.Add(GameEvent.PlayerRequireCard, trigger);
            }
        }

        public BaGuaZhen()
        {
            EquipmentSkill = new BaGuaZhenSkill();
        }

    }
}

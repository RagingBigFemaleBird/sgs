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
using System.Diagnostics;

namespace Sanguosha.Expansions.Battle.Cards
{
    
    public class BaiYinShiZi : Armor
    {
        
        public class BaiYinShiZiSkill : ArmorTriggerSkill
        {            
            void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
            {
                DamageEventArgs args = eventArgs as DamageEventArgs;
                args.Magnitude = 1;
            }

            public BaiYinShiZiSkill()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => 
                    {
                        var args = a as DamageEventArgs;
                        return (args.Magnitude > 1)
                               && (a.ReadonlyCard == null || ArmorIsValid(Owner, args.Source, a.ReadonlyCard));
                    },
                    Run,
                    TriggerCondition.OwnerIsTarget
                );
                Triggers.Add(GameEvent.DamageInflicted, trigger);
                IsEnforced = true;
            }

            public override Player Owner
            {
                get
                {
                    return base.Owner;
                }
                set
                {
                    if (base.Owner == value) return;
                    if (value != null)
                    {
                        Game.CurrentGame.RegisterTrigger(GameEvent.CardsLost, new BaiYinShiZiRegen(ParentEquipment.ParentCard, value));
                    }
                    base.Owner = value;
                }
            }
        }

        public BaiYinShiZi()
        {
            EquipmentSkill = new BaiYinShiZiSkill() { ParentEquipment = this };
        }

    }

    public class BaiYinShiZiRegen : Trigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            foreach (Card c in eventArgs.Cards)
            {
                if (c == theCard && c.HistoryPlace1.DeckType == DeckType.Equipment && c.HistoryPlace1.Player == thePlayer)
                {
                    Game.CurrentGame.RecoverHealth(thePlayer, thePlayer, 1);
                    Game.CurrentGame.UnregisterTrigger(GameEvent.CardsLost, this);
                }
            }
        }

        Card theCard;
        Player thePlayer;
        public BaiYinShiZiRegen(Card c, Player p)
        {
            theCard = c;
            thePlayer = p;
        }
    }
}

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
    
    public class QiLinGong : Weapon
    {
        public QiLinGong()
        {
            EquipmentSkill = new QiLinGongSkill() { ParentEquipment = this };
        }

        
        public class QiLinGongSkill : TriggerSkill, IEquipmentSkill
        {
            public Equipment ParentEquipment { get; set; }
            protected void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
            {
                var equipDeck = Game.CurrentGame.Decks[eventArgs.Targets[0], DeckType.Equipment];
                Card c;
                if (equipDeck.Count(s => (s.Type is DefensiveHorse) || (s.Type is OffensiveHorse)) > 1)
                {
                    int answer = 0;
                    Game.CurrentGame.UiProxies[Owner].AskForMultipleChoice(new MultipleChoicePrompt("QiLinGong", this), new List<OptionPrompt> { new OptionPrompt("JiaYiZuoJi"), new OptionPrompt("JianYiZuoJi") },
                        out answer);
                    if (answer == 1)
                    {
                        var results = from equip in equipDeck where equip.Type is OffensiveHorse select equip;
                        Trace.Assert(results.Count() == 1);
                        c = results.First();
                    }
                    else
                    {
                        Trace.Assert(answer == 0);
                        var results = from equip in equipDeck where equip.Type is DefensiveHorse select equip;
                        Trace.Assert(results.Count() == 1);
                        c = results.First();
                    }
                }
                else
                {
                    var results = from equip in equipDeck where (equip.Type is DefensiveHorse) || (equip.Type is OffensiveHorse) select equip;
                    Trace.Assert(results.Count() == 1);
                    c = results.First();
                }
                Game.CurrentGame.HandleCardDiscard(eventArgs.Targets[0], new List<Card>() { c });
            }
            public QiLinGongSkill()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return a.ReadonlyCard != null && (a.ReadonlyCard.Type is Sha) && (a as DamageEventArgs).OriginalTarget == a.Targets[0] && (Game.CurrentGame.Decks[a.Targets[0], DeckType.Equipment].Any(s => (s.Type is DefensiveHorse) || (s.Type is OffensiveHorse))); },
                    Run,
                    TriggerCondition.OwnerIsSource
                );
                Triggers.Add(GameEvent.DamageCaused, trigger);
            }
        }

        public override int AttackRange
        {
            get { return 5; }
        }

        protected override void RegisterWeaponTriggers(Player p)
        {
            return;
        }

        protected override void UnregisterWeaponTriggers(Player p)
        {
            return;
        }

    }
}

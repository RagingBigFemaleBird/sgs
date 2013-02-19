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
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Basic.Cards
{
    
    public class HanBingJian : Weapon
    {
        public HanBingJian()
        {
            EquipmentSkill = new HanBingJianSkill() { ParentEquipment = this };
        }

        
        public class HanBingJianSkill : TriggerSkill, IEquipmentSkill
        {
            public Equipment ParentEquipment { get; set; }
            protected void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Core.Utils.GameDelays.Delay(Core.Utils.GameDelayTypes.HanBingJian);
                Player dest = eventArgs.Targets[0];
                for (int i = 0; i < 2 && Game.CurrentGame.Decks[dest, DeckType.Hand].Concat(Game.CurrentGame.Decks[dest, DeckType.Equipment]).Count() > 0; i++)
                {
                    IUiProxy ui = Game.CurrentGame.UiProxies[Owner];
                    if (Owner.IsDead) return;
                    List<DeckPlace> places = new List<DeckPlace>();
                    places.Add(new DeckPlace(dest, DeckType.Hand));
                    places.Add(new DeckPlace(dest, DeckType.Equipment));
                    List<string> resultDeckPlace = new List<string>();
                    resultDeckPlace.Add("HanBing");
                    List<int> resultDeckMax = new List<int>();
                    resultDeckMax.Add(1);
                    List<List<Card>> answer;
                    if (!ui.AskForCardChoice(new CardChoicePrompt("HanBing"), places, resultDeckPlace, resultDeckMax, new RequireOneCardChoiceVerifier(), out answer))
                    {
                        Trace.TraceInformation("Player {0} Invalid answer", Owner.Id);
                        answer = new List<List<Card>>();
                        answer.Add(new List<Card>());
                        var collection = Game.CurrentGame.Decks[dest, DeckType.Hand].Concat
                                         (Game.CurrentGame.Decks[dest, DeckType.Equipment]);
                        answer[0].Add(collection.First());
                    }
                    Trace.Assert(answer.Count == 1 && answer[0].Count == 1);
                    Game.CurrentGame.HandleCardDiscard(dest, answer[0]);
                }
                throw new TriggerResultException(TriggerResult.End);
            }
            public HanBingJianSkill()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) =>
                    {
                        return (a.ReadonlyCard != null) && (a.ReadonlyCard.Type is Sha) && (a as DamageEventArgs).OriginalTarget == a.Targets[0] && a.Targets[0].HandCards().Count + a.Targets[0].Equipments().Count > 0;
                    },
                    Run,
                    TriggerCondition.OwnerIsSource
                );
                Triggers.Add(GameEvent.DamageCaused, trigger);
            }
        }

        public override int AttackRange
        {
            get { return 2; }
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

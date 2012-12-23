using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using System.Diagnostics;

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 烈刃-每当你使用【杀】对目标角色造成一次伤害后，可与其拼点，若你赢，你获得该角色的一张牌。
    /// </summary>
    public class LieRen : TriggerSkill
    {
        protected void Run(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            var result = Game.CurrentGame.PinDian(owner, eventArgs.Targets[0]);
            if (result && Game.CurrentGame.Decks[eventArgs.Targets[0], DeckType.Hand].Count + Game.CurrentGame.Decks[eventArgs.Targets[0], DeckType.Equipment].Count > 0)
            {
                var dest = eventArgs.Targets[0];
                List<DeckPlace> places = new List<DeckPlace>();
                places.Add(new DeckPlace(dest, DeckType.Hand));
                places.Add(new DeckPlace(dest, DeckType.Equipment));
                List<string> resultDeckPlace = new List<string>();
                resultDeckPlace.Add("LieRen");
                List<int> resultDeckMax = new List<int>();
                resultDeckMax.Add(1);
                List<List<Card>> answer;
                if (!Game.CurrentGame.UiProxies[owner].AskForCardChoice(new CardChoicePrompt("LieRen"), places, resultDeckPlace, resultDeckMax, new RequireOneCardChoiceVerifier(true), out answer))
                {
                    Trace.TraceInformation("Player {0} Invalid answer", owner.Id);
                    answer = new List<List<Card>>();
                    answer.Add(new List<Card>());
                    var collection = Game.CurrentGame.Decks[dest, DeckType.Hand].Concat
                                     (Game.CurrentGame.Decks[dest, DeckType.DelayedTools].Concat
                                     (Game.CurrentGame.Decks[dest, DeckType.Equipment]));
                    answer[0].Add(collection.First());
                }
                Card theCard = answer[0][0];
                Game.CurrentGame.SyncCardAll(ref theCard);
                Trace.Assert(answer.Count == 1 && answer[0].Count == 1);

                Game.CurrentGame.HandleCardTransferToHand(dest, owner, new List<Card>() { theCard });
            }
        }

        public LieRen()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Game.CurrentGame.Decks[p, DeckType.Hand].Count > 0 && !a.Targets[0].IsDead && Game.CurrentGame.Decks[a.Targets[0], DeckType.Hand].Count > 0; },
                Run,
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.AfterDamageCaused, trigger);
            IsAutoInvoked = false;
        }
    }
}

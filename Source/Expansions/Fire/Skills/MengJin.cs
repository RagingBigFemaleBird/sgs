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
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Fire.Skills
{
    /// <summary>
    /// 猛进-当你使用的【杀】被目标角色的【闪】抵消时，你可以弃置其一张牌。
    /// </summary>
    public class MengJin : TriggerSkill
    {
        protected void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            Player dest = eventArgs.Targets[0];
            IUiProxy ui = Game.CurrentGame.UiProxies[Owner];
            List<DeckPlace> places = new List<DeckPlace>();
            places.Add(new DeckPlace(dest, DeckType.Hand));
            places.Add(new DeckPlace(dest, DeckType.Equipment));
            places.Add(new DeckPlace(dest, DeckType.DelayedTools));
            List<string> resultDeckPlace = new List<string>();
            resultDeckPlace.Add("MengJin");
            List<int> resultDeckMax = new List<int>();
            resultDeckMax.Add(1);
            List<List<Card>> answer;

            if (!ui.AskForCardChoice(new CardChoicePrompt("MengJin", Owner), places, resultDeckPlace, resultDeckMax, new RequireOneCardChoiceVerifier(), out answer))
            {
                Trace.TraceInformation("Player {0} Invalid answer", Owner);
                answer = new List<List<Card>>();
                answer.Add(Game.CurrentGame.PickDefaultCardsFrom(places));
            }
            Card theCard = answer[0][0];
            Trace.Assert(answer.Count == 1 && answer[0].Count == 1);

            Game.CurrentGame.HandleCardDiscard(dest, new List<Card>() { theCard });
        }

        public MengJin()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Targets[0].HandCards().Count + a.Targets[0].Equipments().Count > 0; },
                Run,
                TriggerCondition.OwnerIsSource
            ) { };
            Triggers.Add(ShaCancelling.PlayerShaTargetDodged, trigger);
        }
    }
}

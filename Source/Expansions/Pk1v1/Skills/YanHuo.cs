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

namespace Sanguosha.Expansions.Pk1v1.Skills
{
    /// <summary>
    /// 延祸-你死亡时，你可以依次弃置对手的X张牌（X为你死亡时拥有牌的数量）。
    /// </summary>
    public class YanHuo : TriggerSkill
    {
        public YanHuo()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return true; },
                (p, e, a) =>
                {
                    int X = p.HandCards().Count + p.Equipments().Count;
                    Player opponent = Game.CurrentGame.Players.First(pl => pl != p);
                    Trace.Assert(opponent != null);
                    while (X-- > 0 && opponent.HandCards().Count + opponent.Equipments().Count > 0)
                    {
                        List<DeckPlace> decks = new List<DeckPlace>() { new DeckPlace(opponent, DeckType.Hand), new DeckPlace(opponent, DeckType.Equipment) };
                        List<List<Card>> answer;
                        if (!p.AskForCardChoice(new CardChoicePrompt("YanHuo"), decks, new List<string>() { "YanHuo" }, new List<int>() { 1 }, new RequireOneCardChoiceVerifier(true), out answer))
                        {
                            answer = new List<List<Card>>();
                            answer.Add(Game.CurrentGame.PickDefaultCardsFrom(decks));
                        }
                        Game.CurrentGame.HandleCardDiscard(opponent, answer[0]);
                    }
                },
                TriggerCondition.OwnerIsTarget
            );

            Triggers.Add(GameEvent.PlayerIsDead, trigger);
            IsAutoInvoked = true;
        }
    }
}

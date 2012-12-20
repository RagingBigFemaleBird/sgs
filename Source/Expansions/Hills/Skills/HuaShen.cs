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
using Sanguosha.Core.Heroes;

namespace Sanguosha.Expansions.Hills.Skills
{
    /// <summary>
    /// 化身-所有人都展示武将牌后，你随机获得两张未加入游戏的武将牌，称为"化身牌"，选一张置于你面前并声明该武将的一项技能。你获得该技能且同时将性别和势力属性变成与该武将相同直到"化身牌"被替换。在你的每个回合开始时和结束后，你可以替换"化身牌"，然后你为当前的"化身牌"重新声明一项技能(你不可声明限定技、觉醒技或主公技)。
    /// </summary>
    public class HuaShen : TriggerSkill
    {
        static PrivateDeckType HuaShenDeck = new PrivateDeckType("HuaShen");

        public static void AcquireHeroCard(Player player)
        {
            Card card = Game.CurrentGame.Decks[DeckType.Heroes][0];
            Game.CurrentGame.SyncImmutableCard(player, card);
            CardsMovement move = new CardsMovement();
            move.cards = new List<Card>() {card};
            move.to = new DeckPlace(player, HuaShenDeck);
            Game.CurrentGame.MoveCards(move, null);
        }

        ISkill acquiredSkill;

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (acquiredSkill != null)
                Owner.LoseAdditionalSkill(acquiredSkill);
            List<List<Card>> answer;
            if (!Game.CurrentGame.UiProxies[Owner].AskForCardChoice(
                new CardChoicePrompt("HuaShen"),
                new List<DeckPlace>() { new DeckPlace(Owner, HuaShenDeck) },
                new List<string>() { "HuaShen" },
                new List<int>() { 1 },
                new RequireOneCardChoiceVerifier(),
                out answer))
            {
                Trace.TraceInformation("Invalid answer, choosing for you");
                answer = new List<List<Card>>();
                answer.Add(new List<Card>());
                answer[0].Add(Game.CurrentGame.Decks[Owner, HuaShenDeck][0]);
            }
            Game.CurrentGame.SyncImmutableCardAll(answer[0][0]);
            Trace.Assert(answer[0][0].Type is HeroCardHandler);
            var handler = answer[0][0].Type as HeroCardHandler;
            List<ISkill> skills = new List<ISkill>();
            List<string> hsOptions = new List<string>();
            foreach (var sk in handler.Hero.Skills)
            {
                if (sk.IsAwakening || sk.IsRulerOnly || sk.IsSingleUse) continue;
                skills.Add(sk);
                hsOptions.Add(string.Format("Skill.{0}.Name", sk.GetType().Name));
            }
            int skanswer;
            Game.CurrentGame.UiProxies[Owner].AskForMultipleChoice(new MultipleChoicePrompt("HuaShen"), hsOptions, out skanswer);
            acquiredSkill = skills[skanswer];
            Owner.AcquireAdditionalSkill(acquiredSkill);
            Owner.Allegiance = handler.Hero.Allegiance;
            Owner.IsMale = handler.Hero.IsMale;
            Owner.IsFemale = !handler.Hero.IsMale;
            return;
        }

        public HuaShen()
        {
            acquiredSkill = null;
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { AcquireHeroCard(p); AcquireHeroCard(p); Run(p, e, a); },
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false };
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PlayerGameStartAction, trigger);
            Triggers.Add(GameEvent.PhaseProceedEvents[TurnPhase.BeforeStart], trigger2);
            Triggers.Add(GameEvent.PhaseEndEvents[TurnPhase.PostEnd], trigger2);
            IsAutoInvoked = false;
        }
    }
}

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
        protected override int GenerateSpecialEffectHintIndex(Player source, List<Player> targets)
        {
            if (Owner.IsFemale) return 1;
            return 0;
        }

        static PrivateDeckType HuaShenDeck = new PrivateDeckType("HuaShen");

        public static void AcquireHeroCard(Player player, Hero tag)
        {
            Card card = Game.CurrentGame.Decks[DeckType.Heroes][0];
            Game.CurrentGame.SyncImmutableCard(player, card);
            CardsMovement move = new CardsMovement();
            move.Cards = new List<Card>() { card };
            move.To = new DeckPlace(player, HuaShenDeck);
            move.Helper.PrivateDeckHeroTag = tag;
            Game.CurrentGame.MoveCards(move);
        }

        ISkill acquiredSkill;

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill tempSkill = null;
            if (acquiredSkill != null)
                tempSkill = acquiredSkill;
            List<List<Card>> answer;
            if (!Game.CurrentGame.UiProxies[Owner].AskForCardChoice(
                new CardChoicePrompt("HuaShen", Owner),
                new List<DeckPlace>() { new DeckPlace(Owner, HuaShenDeck) },
                new List<string>() { "HuaShen" },
                new List<int>() { 1 },
                new RequireOneCardChoiceVerifier(false, true),
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
            List<OptionPrompt> hsOptions = new List<OptionPrompt>();
            foreach (var sk in handler.Hero.Skills)
            {
                if (sk.IsAwakening || sk.IsRulerOnly || sk.IsSingleUse) continue;
                skills.Add(sk);
                hsOptions.Add(new OptionPrompt("HuaShen", sk));
            }
            int skanswer;
            Game.CurrentGame.UiProxies[Owner].AskForMultipleChoice(new MultipleChoicePrompt("HuaShen"), hsOptions, out skanswer);
            acquiredSkill = skills[skanswer];
            Game.CurrentGame.NotificationProxy.NotifyImpersonation(Owner, HeroTag, handler.Hero, acquiredSkill);
            if (Game.CurrentGame.IsMainHero(HeroTag, Owner))
            {
                Owner.Allegiance = handler.Hero.Allegiance;
                Owner.IsMale = handler.Hero.IsMale;
                Owner.IsFemale = !handler.Hero.IsMale;
                Game.CurrentGame.HandleGodHero(Owner);
                Game.CurrentGame.Emit(GameEvent.PlayerChangedAllegiance, new GameEventArgs() { Source = Owner });
            }
            Game.CurrentGame.PlayerAcquireAdditionalSkill(Owner, acquiredSkill, HeroTag);
            if (tempSkill != null)
                Game.CurrentGame.PlayerLoseAdditionalSkill(Owner, tempSkill);
            return;
        }

        void LoseHuaShen(Player Owner)
        {
            Owner.Allegiance = Allegiance.Qun;
            Owner.IsMale = true;
            Owner.IsFemale = !Owner.IsMale;
            Game.CurrentGame.NotificationProxy.NotifyImpersonation(Owner, HeroTag, null, null);
            Game.CurrentGame.Emit(GameEvent.PlayerChangedAllegiance, new GameEventArgs() { Source = Owner });
            if (acquiredSkill != null && Owner.AdditionalSkills.Contains(acquiredSkill))
                Game.CurrentGame.PlayerLoseAdditionalSkill(Owner, acquiredSkill);
        }

        public override Player Owner
        {
            get
            {
                return base.Owner;
            }
            set
            {
                Player original = base.Owner;
                base.Owner = value;
                if (base.Owner == null && original != null)
                {
                    LoseHuaShen(original);
                }
            }
        }

        public HuaShen()
        {
            acquiredSkill = null;
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    AcquireHeroCard(p, HeroTag);
                    AcquireHeroCard(p, HeroTag);
                    Run(p, e, a);
                },
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false };
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Game.CurrentGame.Decks[Owner, HuaShenDeck].Count > 0; },
                Run,
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PlayerGameStartAction, trigger);
            Triggers.Add(GameEvent.PhaseProceedEvents[TurnPhase.BeforeStart], trigger2);
            Triggers.Add(GameEvent.PhasePostEnd, trigger2);

            IsAutoInvoked = false;
            DeckCleanup.Add(HuaShenDeck);
        }
    }
}

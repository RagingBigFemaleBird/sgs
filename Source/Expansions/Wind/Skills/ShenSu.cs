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
    /// 神速-你可以选择一至两项：
　　///   1.跳过你此回合的判定阶段和摸牌阶段。
　　///   2.跳过你此回合出牌阶段并弃置一张装备牌。
　　///   你每选择一项，视为对一名其他角色使用一张【杀】。
    /// </summary>
    public class ShenSu : TriggerSkill
    {
        public class ShenSuStage2Verifier : CardsAndTargetsVerifier
        {
            public ShenSuStage2Verifier()
            {
                MinPlayers = 1;
                MaxPlayers = 1;
                MinCards = 1;
                MaxCards = 1;
                Discarding = true;
            }

            protected override bool VerifyPlayer(Player source, Player player)
            {
                return player != source;
            }

            protected override bool VerifyCard(Player source, Card card)
            {
                return CardCategoryManager.IsCardCategory(card.Type.Category, CardCategory.Equipment);
            }
        }

        void Run1(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("ShenSu1"), new OneTargetNoSelfVerifier(),
                out skill, out cards, out players))
            {
                NotifySkillUse(players);
                GameEventArgs args = new GameEventArgs();
                args.Source = Owner;
                args.Targets = players;
                args.Skill = new CardWrapper(Owner, new RegularSha());
                args.Cards = new List<Card>();
                Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
                Game.CurrentGame.CurrentPhase++;
                Game.CurrentGame.CurrentPhase++;
                Game.CurrentGame.CurrentPhaseEventIndex = 2;
            }
        }

        void Run2(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("ShenSu2"), new ShenSuStage2Verifier(),
                out skill, out cards, out players))
            {
                NotifySkillUse(players);
                Game.CurrentGame.HandleCardDiscard(Owner, cards);
                GameEventArgs args = new GameEventArgs();
                args.Source = Owner;
                args.Targets = players;
                args.Skill = new CardWrapper(Owner, new RegularSha());
                args.Cards = new List<Card>();
                Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
                Game.CurrentGame.CurrentPhase++;
                Game.CurrentGame.CurrentPhaseEventIndex = 2;
            }
        }

        public ShenSu()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                Run1,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                Run2,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };

            Triggers.Add(GameEvent.PhaseOutEvents[TurnPhase.Start], trigger);
            Triggers.Add(GameEvent.PhaseOutEvents[TurnPhase.Draw], trigger2);
            IsAutoInvoked = null;
        }

    }
}

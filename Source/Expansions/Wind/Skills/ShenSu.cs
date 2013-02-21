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
        class ShenSuVerifier : CardUsageVerifier
        {
            public override VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                return FastVerify(source, skill, cards, players);
            }

            public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if (!shensu1)
                {
                    if (cards == null || cards.Count == 0)
                    {
                        if (players != null && players.Count > 0)
                        {
                            return VerifierResult.Fail;
                        }
                        return VerifierResult.Partial;
                    }

                    if (skill is IEquipmentSkill)
                    {
                        return VerifierResult.Fail;
                    }

                    if (!cards[0].Type.IsCardCategory(CardCategory.Equipment))
                    {
                        return VerifierResult.Fail;
                    }

                    if (cards.Count > 1)
                    {
                        return VerifierResult.Fail;
                    }

                    if (!Game.CurrentGame.PlayerCanDiscardCard(source, cards[0]))
                    {
                        return VerifierResult.Fail;
                    }

                }
                else
                {
                    if (cards.Count > 0) return VerifierResult.Fail;
                }
                return verifier.FastVerify(source, skill, new List<Card>(), players);
            }

            public override IList<CardHandler> AcceptableCardTypes
            {
                get { return null; }
            }

            bool shensu1;
            DummyShaVerifier verifier;
            public ShenSuVerifier(bool ShenSu1, DummyShaVerifier verifier)
            {
                shensu1 = ShenSu1;
                this.verifier = verifier;
            }
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            bool shensu1 = gameEvent == GameEvent.PhaseOutEvents[TurnPhase.Start];
            CardUsagePrompt shensu1Prompt = new CardUsagePrompt("ShenSu1", this);
            CardUsagePrompt shensu2Prompt = new CardUsagePrompt("ShenSu2", this);
            if (Owner.AskForCardUsage(shensu1 ? shensu1Prompt : shensu2Prompt, new ShenSuVerifier(shensu1, verifier), out skill, out cards, out players))
            {
                NotifySkillUse();
                if (!shensu1) Game.CurrentGame.HandleCardDiscard(Owner, cards);
                GameEventArgs args = new GameEventArgs();
                Owner[Sha.NumberOfShaUsed]--;
                args.Source = Owner;
                args.Targets = players;
                args.Skill = skill == null ? new CardWrapper(Owner, new RegularSha(), false) : skill;
                args.Cards = new List<Card>();
                CardTransformSkill transformSkill = skill as CardTransformSkill;
                if (transformSkill != null)
                {
                    CompositeCard card;
                    transformSkill.TryTransform(new List<Card>() { new Card() { Type = new RegularSha(), Place = new DeckPlace(null, DeckType.None) } }, null, out card);
                    card.Subcards.Clear();
                    args.Card = card;
                }
                Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
                Game.CurrentGame.CurrentPhase++;
                if (shensu1) Game.CurrentGame.CurrentPhase++;
                Game.CurrentGame.CurrentPhaseEventIndex = 2;
            }
        }

        DummyShaVerifier verifier;
        public ShenSu()
        {
            verifier = new Basic.Cards.DummyShaVerifier(null, new RegularSha(), ShenSuSha);
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };

            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Card[ShenSuSha] != 0; },
                (p, e, a) =>
                {
                    ShaEventArgs args = a as ShaEventArgs;
                    args.RangeApproval[0] = true;
                },
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };

            Triggers.Add(GameEvent.PhaseOutEvents[TurnPhase.Start], trigger);
            Triggers.Add(GameEvent.PhaseOutEvents[TurnPhase.Draw], trigger);
            Triggers.Add(Sha.PlayerShaTargetValidation, trigger2);
            IsAutoInvoked = null;
        }

        public static CardAttribute ShenSuSha = CardAttribute.Register("ShenSuSha");
    }
}

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
using Sanguosha.Expansions.Basic.Skills;
using Sanguosha.Expansions.Woods.Skills;

namespace Sanguosha.Expansions.Hills.Skills
{
    /// <summary>
    /// 拜印—觉醒技，回合开始阶段，若你拥有4个以上或更多的忍标记，须减1点体力上限，并永久获得技能“极略”(弃一枚忍标记来发动下列一项技能---“鬼才”、“放逐”、“完杀”、“制衡”、“集智”)。
    /// </summary>
    public class BaiYin : TriggerSkill
    {
        public BaiYin()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return p[BaiYinAwaken] == 0 && p[RenJie.RenMark] >= 4; },
                (p, e, a) =>
                {
                    p[BaiYinAwaken] = 1;
                    Game.CurrentGame.LoseMaxHealth(p, 1);
                    Game.CurrentGame.PlayerAcquireAdditionalSkill(p, new JiLveFangZhu(), HeroTag);
                    Game.CurrentGame.PlayerAcquireAdditionalSkill(p, new JiLveGuiCai(), HeroTag);
                    Game.CurrentGame.PlayerAcquireAdditionalSkill(p, new JiLveJiZhi(), HeroTag);
                    Game.CurrentGame.PlayerAcquireAdditionalSkill(p, new JiLveWanSha(), HeroTag);
                    Game.CurrentGame.PlayerAcquireAdditionalSkill(p, new JiLveZhiHeng(), HeroTag);
                },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Start], trigger);
            IsAwakening = true;
        }

        public static PlayerAttribute BaiYinAwaken = PlayerAttribute.Register("BaiYinAwaken", false);

        class JiLveGuiCai : GuiCai
        {

            protected override void OnJudgeBegin(Player player, GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (player[RenJie.RenMark] == 0)
                {
                    return;
                }
                if (Game.CurrentGame.Decks[player, DeckType.Hand].Count == 0)
                {
                    return;
                }
                ISkill skill;
                List<Card> cards;
                List<Player> players;
                Card c = Game.CurrentGame.Decks[eventArgs.Source, DeckType.JudgeResult].Last();
                if (Game.CurrentGame.UiProxies[player].AskForCardUsage(new CardUsagePrompt("GuiCai", eventArgs.Source, c.Suit, c.Rank), new GuiCaiVerifier(), out skill, out cards, out players))
                {
                    NotifySkillUse();
                    player[RenJie.RenMark]--;
                    ReplaceJudgementCard(player, eventArgs.Source, cards[0], this);
                }
            }

            public JiLveGuiCai()
            {
                Triggers.Clear();
                Triggers.Add(GameEvent.PlayerJudgeBegin, new RelayTrigger(OnJudgeBegin, TriggerCondition.Global));
            }
        }

        class JiLveFangZhu : FangZhu
        {
            protected void Wrapper(Player player, GameEvent gameEvent, GameEventArgs eventArgs, List<Card> cards, List<Player> players)
            {
                player[RenJie.RenMark]--;
                OnAfterDamageInflicted(player, gameEvent, eventArgs, cards, players);
            }

            public JiLveFangZhu()
            {
                Triggers.Clear();
                var trigger = new AutoNotifyUsagePassiveSkillTrigger(
                    this,
                    (p, e, a) => { return p[RenJie.RenMark] > 0; },
                    Wrapper,
                    TriggerCondition.OwnerIsTarget,
                    new FangZhuVerifier()
                );
                Triggers.Add(GameEvent.AfterDamageInflicted, trigger);
                IsAutoInvoked = null;
            }
        }

        class JiLveJiZhi : TriggerSkill
        {
            public JiLveJiZhi()
            {
                Triggers.Add(GameEvent.PlayerUsedCard, new AutoNotifyPassiveSkillTrigger(this,
                    (p, e, a) => { return p[RenJie.RenMark] > 0 && CardCategoryManager.IsCardCategory(a.Card.Type.Category, CardCategory.ImmediateTool); },
                    (p, e, a) => { p[RenJie.RenMark]--; Game.CurrentGame.DrawCards(p, 1); },
                    TriggerCondition.OwnerIsSource
                ) { Type = TriggerType.Skill });
            }
        }

        class JiLveWanSha : AutoVerifiedActiveSkill
        {
            public JiLveWanSha()
            {
                MinCards = 0;
                MaxCards = 0;
                MinPlayers = 0;
                MaxPlayers = 0;
                Helper.HasNoConfirmation = true;
            }

            protected override bool VerifyCard(Player source, Card card)
            {
                return true;
            }

            protected override bool VerifyPlayer(Player source, Player player)
            {
                return true;
            }

            protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
            {
                return source[RenJie.RenMark] > 0 && source[BaiYinWanShaUsed] == 0;
            }
            static PlayerAttribute WanShaStatus = PlayerAttribute.Register("JiLveWanSha", false, false, true);

            class WanShaRemoval : Trigger
            {
                ISkill skill;
                public WanShaRemoval(Player owner, ISkill s)
                {
                    Owner = owner;
                    skill = s;
                }

                public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
                {
                    if (eventArgs.Source == Owner)
                    {
                        Owner[WanShaStatus] = 0;
                        Owner.LoseAdditionalSkill(skill);
                        Game.CurrentGame.UnregisterTrigger(GameEvent.PhasePostEnd, this);
                    }
                }
            }

            public override bool Commit(GameEventArgs arg)
            {
                arg.Source[RenJie.RenMark]--;
                arg.Source[BaiYinWanShaUsed] = 1;
                arg.Source[WanShaStatus] = 1;
                ISkill skill = new WanSha();
                Game.CurrentGame.PlayerAcquireAdditionalSkill(arg.Source, skill, HeroTag);
                Game.CurrentGame.RegisterTrigger(GameEvent.PhasePostEnd, new WanShaRemoval(arg.Source, skill));
                return true;
            }

            public static PlayerAttribute BaiYinWanShaUsed = PlayerAttribute.Register("BaiYinWanShaUsed", true);

        }

        class JiLveZhiHeng : ZhiHeng
        {
            public override VerifierResult Validate(GameEventArgs arg)
            {
                if (arg.Source[RenJie.RenMark] == 0)
                {
                    return VerifierResult.Fail;
                }
                return base.Validate(arg);
            }

            public override bool Commit(GameEventArgs arg)
            {
                arg.Source[RenJie.RenMark]--;
                return base.Commit(arg);
            }
        }
    }
}

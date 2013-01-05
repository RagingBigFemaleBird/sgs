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

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    /// <summary>
    /// 将驰-摸牌阶段，你可以选择一项：
    /// 1、额外摸一张牌，若如此做，你不能使用或打出【杀】，直到回合结束。
    /// 2、少摸一张牌，若如此做，出牌阶段你使用【杀】时无距离限制且你可以额外使用一张【杀】，直到回合结束。
    /// </summary>
    public class JiangChi : TriggerSkill
    {
        public class JiangChi1 : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                {
                    return;
                }
                if (eventArgs.Card.Type is Sha)
                {
                    throw new TriggerResultException(TriggerResult.Fail);
                }
            }
            public JiangChi1(Player p)
            {
                Owner = p;
            }
        }

        public class JiangChi2 : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                ShaEventArgs args = (ShaEventArgs)eventArgs;
                Trace.Assert(args != null);
                if (args.Source != Owner)
                {
                    return;
                }
                for (int i = 0; i < args.RangeApproval.Count; i++)
                {
                    args.RangeApproval[i] = true;
                }
            }
            public JiangChi2(Player p)
            {
                Owner = p;
            }
        }

        public class JiangChiRemoval : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                {
                    return;
                }
                if (isJiangChi1)
                {
                    Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerCanUseCard, trigger);
                    Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerCanPlayCard, trigger);
                }
                else
                {
                    Game.CurrentGame.UnregisterTrigger(Sha.PlayerShaTargetValidation, trigger);
                }
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhasePostEnd, this);
            }

            Trigger trigger;
            bool isJiangChi1;
            public JiangChiRemoval(Player p, Trigger t, bool ChoiceFirst)
            {
                Owner = p;
                trigger = t;
                isJiangChi1 = ChoiceFirst;
            }
        }

        public void Run(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            Trigger tri = new Trigger();
            int answer = 0;
            List<OptionPrompt> JiangChiQuestion = new List<OptionPrompt>();
            JiangChiQuestion.Add(Prompt.NoChoice);
            JiangChiQuestion.Add(new OptionPrompt("JiangChi1"));
            JiangChiQuestion.Add(new OptionPrompt("JiangChi2"));
            owner.AskForMultipleChoice(new MultipleChoicePrompt("JiangChi"), JiangChiQuestion, out answer);
            if (answer == 0)
            {
                return;
            }
            NotifySkillUse(new List<Player>());
            if (answer == 1)
            {
                owner[Player.DealAdjustment]++;
                tri = new JiangChi1(owner);
                Game.CurrentGame.RegisterTrigger(GameEvent.PlayerCanUseCard, tri);
                Game.CurrentGame.RegisterTrigger(GameEvent.PlayerCanPlayCard, tri);
            }
            else
            {
                owner[Sha.AdditionalShaUsable]++;
                owner[Player.DealAdjustment]--;
                tri = new JiangChi2(owner);
                Game.CurrentGame.RegisterTrigger(Sha.PlayerShaTargetValidation, tri);
            }
            Trigger triRemoval = new JiangChiRemoval(owner, tri, answer == 1);
            Game.CurrentGame.RegisterTrigger(GameEvent.PhasePostEnd, triRemoval);
        }

        public JiangChi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseProceedEvents[TurnPhase.Draw], trigger);
            IsAutoInvoked = null;
        }
    }
}

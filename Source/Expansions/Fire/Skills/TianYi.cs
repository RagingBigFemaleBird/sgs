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

namespace Sanguosha.Expansions.Fire.Skills
{
    /// <summary>
    /// 天义―出牌阶段，你可以与一名其他角色拼点。若你赢，你获得以下技能直到回合结束：你使用【杀】时无距离限制；可额外使用一张【杀】；使用【杀】时可额外指定一个目标。若你没赢，你不能使用【杀】，直到回合结束。每阶段限一次.
    /// </summary>
    public class TianYi : ActiveSkill
    {
        public override VerifierResult Validate(GameEventArgs arg)
        {
            if (Owner[TianYiUsed] != 0)
            {
                return VerifierResult.Fail;
            }
            List<Card> cards = arg.Cards;
            if (cards != null && cards.Count > 0)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets != null && arg.Targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets == null || arg.Targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (arg.Targets[0] == Owner || Game.CurrentGame.Decks[arg.Targets[0], DeckType.Hand].Count == 0)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public class TianYiWinTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                ShaEventArgs args = (ShaEventArgs)eventArgs;
                Trace.Assert(args != null);
                if (args.Source != Owner)
                {
                    return;
                }
                if (args.TargetApproval[0])
                {
                    for (int i = 1; i < args.TargetApproval.Count; i++)
                    {
                        if (!args.TargetApproval[i])
                        {
                            args.TargetApproval[i] = true;
                            break;
                        }
                    }
                }
                for (int i = 0; i < args.RangeApproval.Count; i++)
                {
                    args.RangeApproval[i] = true;
                }
            }

            public TianYiWinTrigger(Player p)
            {
                Owner = p;
            }
        }

        public class TianYiLoseTrigger : Trigger
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

            public TianYiLoseTrigger(Player p)
            {
                Owner = p;
            }
        }

        class TianYiRemoval : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                {
                    return;
                }
                if (winTrigger != null)
                {
                    Game.CurrentGame.UnregisterTrigger(Sha.PlayerShaTargetValidation, winTrigger);
                }
                if (loseTrigger != null)
                {
                    Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerCanUseCard, loseTrigger);
                }
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhaseEndEvents[TurnPhase.End], this);
            }

            Trigger winTrigger;
            Trigger loseTrigger;
            public TianYiRemoval(Player p, Trigger win, Trigger lose)
            {
                Owner = p;
                winTrigger = win;
                loseTrigger = lose;
            }
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[TianYiUsed] = 1;
            bool result = Game.CurrentGame.PinDian(Owner, arg.Targets[0]);
            if (result)
            {
                Owner[Sha.AdditionalShaUsable]++;
                var trig = new TianYiWinTrigger(Owner);
                Game.CurrentGame.RegisterTrigger(Sha.PlayerShaTargetValidation, trig);
                Game.CurrentGame.RegisterTrigger(GameEvent.PhaseEndEvents[TurnPhase.End], new TianYiRemoval(Owner, trig, null));
            }
            else
            {
                var trig = new TianYiLoseTrigger(Owner);
                Game.CurrentGame.RegisterTrigger(GameEvent.PlayerCanUseCard, trig);
                Game.CurrentGame.RegisterTrigger(GameEvent.PhaseEndEvents[TurnPhase.End], new TianYiRemoval(Owner, null, trig));
            }
            return true;
        }

        private static PlayerAttribute TianYiUsed = PlayerAttribute.Register("TianYiUsed", true);

    }
}

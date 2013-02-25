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
        public TianYi()
        {
            LinkedPassiveSkill = new TianYiPassiveSkill();
        }

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
                Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerCanUseCard, loseTrigger);
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhasePostEnd, this);
            }

            Trigger loseTrigger;
            public TianYiRemoval(Player p, Trigger lose)
            {
                Owner = p;
                loseTrigger = lose;
            }
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[TianYiUsed] = 1;
            var result = Game.CurrentGame.PinDian(Owner, arg.Targets[0], this);
            TianYiPassiveSkill _tyTriggerSkill = LinkedPassiveSkill as TianYiPassiveSkill;
            if (result == true)
            {
                _tyTriggerSkill.TianYiResult = true;
                Owner[Sha.AdditionalShaUsable]++;
            }
            else
            {
                _tyTriggerSkill.TianYiResult = false;
                var loseTrigger = new TianYiLoseTrigger(Owner);
                Game.CurrentGame.RegisterTrigger(GameEvent.PlayerCanUseCard, loseTrigger);
                Game.CurrentGame.RegisterTrigger(GameEvent.PhasePostEnd, new TianYiRemoval(Owner, loseTrigger));
            }
            return true;
        }

        public override Player Owner
        {
            get
            {
                return base.Owner;
            }
            set
            {
                if (base.Owner == value) return;
                var backup = base.Owner;
                base.Owner = value;
                if (backup != null)
                {
                    if ((LinkedPassiveSkill as TianYiPassiveSkill).TianYiResult)
                    {
                        backup[Sha.AdditionalShaUsable]--;
                    }
                }
            }
        }

        class TianYiPassiveSkill : TriggerSkill
        {
            public bool TianYiResult { get; set; }
            public TianYiPassiveSkill()
            {
                TianYiResult = false;
                var winTrigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return p[TianYiUsed] == 1 && TianYiResult; },
                    (p, e, a) =>
                    {
                        ShaEventArgs args = (ShaEventArgs)a;
                        Trace.Assert(args != null);
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
                    },
                    TriggerCondition.OwnerIsSource
                ) { AskForConfirmation = false, IsAutoNotify = false};
                Triggers.Add(Sha.PlayerShaTargetValidation, winTrigger);
            }
        }

        private static PlayerAttribute TianYiUsed = PlayerAttribute.Register("TianYiUsed", true);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;

namespace Sanguosha.Expansions.Basic.Cards
{
    
    public class Sha : CardHandler
    {
        public virtual DamageElement ShaDamageElement
        {
            get { return DamageElement.None; }
        }

        public override void Process(Player source, List<Player> dests, ICard card, ReadOnlyCard readonlyCard)
        {
            source[NumberOfShaUsed]++;
            base.Process(source, dests, card, readonlyCard);
        }

        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard)
        {
            Game.CurrentGame.DoDamage(source, dest, 1, ShaDamageElement, card, readonlyCard);
        }

        public VerifierResult ShaVerifyForJieDaoShaRenOnly(Player source, ICard card, List<Player> targets)
        {
            return Verify(source, card, targets);
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            if (targets != null && targets.Count > 0)
            {
                ShaEventArgs args = new ShaEventArgs();
                args.Source = source;
                args.Card = card;
                args.RangeApproval = new List<bool>(targets.Count);
                args.TargetApproval = new List<bool>(targets.Count);
                foreach (Player t in targets)
                {
                    if (t == source)
                    {
                        return VerifierResult.Fail;
                    }
                    if (Game.CurrentGame.DistanceTo(source, t) <= source[Player.AttackRange] + 1)
                    {
                        args.RangeApproval.Add(true);
                    }
                    else
                    {
                        args.RangeApproval.Add(false);
                    }
                    args.TargetApproval.Add(false);
                }
                if (source[NumberOfShaUsed] <= source[AdditionalShaUsable])
                {
                    args.TargetApproval[0] = true;
                }

                try
                {
                    Game.CurrentGame.Emit(PlayerShaTargetValidation, args);
                }
                catch (TriggerResultException)
                {
                    throw new NotImplementedException();
                }

                foreach (bool b in args.TargetApproval.Concat(args.RangeApproval))
                {
                    if (!b)
                    {
                        return VerifierResult.Fail;
                    }
                }
            }
            if (targets == null || targets.Count == 0)
            {
                if (source[NumberOfShaUsed] <= source[AdditionalShaUsable])
                {
                    return VerifierResult.Partial;
                }
                try
                {
                    GameEventArgs args = new GameEventArgs();
                    args.Source = source;
                    Game.CurrentGame.Emit(PlayerNumberOfShaCheck, args);
                }
                catch (TriggerResultException e)
                {
                    if (e.Status == TriggerResult.Success)
                    {
                        return VerifierResult.Partial;
                    }
                }
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public override CardCategory Category
        {
            get { return CardCategory.Basic; }
        }
        public static PlayerAttribute NumberOfShaUsed = PlayerAttribute.Register("NumberOfShaUsed", true);
        public static PlayerAttribute AdditionalShaUsable = PlayerAttribute.Register("AdditionalShaUsable", true);
        /// <summary>
        /// 玩家使用杀的目标检测
        /// </summary>
        public static readonly GameEvent PlayerShaTargetValidation = new GameEvent("PlayerShaTargetValidation");
        /// <summary>
        /// 是否可以使用杀 
        /// </summary>
        public static readonly GameEvent PlayerNumberOfShaCheck = new GameEvent("PlayerNumberOfShaCheck");
    }


    public class ShaEventArgs : GameEventArgs
    {
        List<bool> rangeApproval;

        public List<bool> RangeApproval
        {
            get { return rangeApproval; }
            set { rangeApproval = value; }
        }

        List<bool> targetApproval;

        public List<bool> TargetApproval
        {
            get { return targetApproval; }
            set { targetApproval = value; }
        }
    }
}

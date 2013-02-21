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

        public override void Process(GameEventArgs handlerArgs)
        {
            handlerArgs.Source[NumberOfShaUsed]++;
            base.Process(handlerArgs);
        }

        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
        {
            Game.CurrentGame.DoDamage(source, dest, 1, ShaDamageElement, card, readonlyCard);
        }

        public VerifierResult VerifyCore(Player source, ICard card, List<Player> targets)
        {
            if (targets != null && targets.Count > 0)
            {
                ShaEventArgs args = new ShaEventArgs();
                args.Source = source;
                args.Card = card;
                args.Targets = targets;
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


        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            return VerifyCore(source, card, targets);
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

        /// <summary>
        /// 某玩家对某玩家视为使用一张虚拟的杀，能被技能转化，影响选择的目标，如疠火，朱雀羽扇
        /// </summary>
        public static void UseDummyShaTo(Player source, Player target, CardHandler shaType, Prompt prompt, CardAttribute helper = null, bool notifyShaSound = true)
        {
            CompositeCard sha = new CompositeCard() { Type = shaType };
            var v1 = new DummyShaVerifier(target, shaType, helper);
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            source.AskForCardUsage(prompt, v1, out skill, out cards, out players);
            GameEventArgs args = new GameEventArgs();
            args.Source = source;
            args.Targets = new List<Player>(players);
            if (target != null) args.Targets.Add(target);
            args.Skill = skill == null ? new CardWrapper(source, shaType, notifyShaSound) : skill;
            args.Cards = cards;
            CompositeCard card = null;
            if (skill != null)
            {
                List<Card> dummyCards = new List<Card>() { new Card() { Type = shaType, Place = new DeckPlace(null, DeckType.None) } };
                (skill as CardTransformSkill).TryTransform(dummyCards, null, out card);
                //虚拟的杀是不能有子卡的。
                card.Subcards.Clear();
            }
            //在触发 CommitActionToTargets 的时候，只有在这里，args.Card才会被赋值，且为CompositeCard
            args.Card = card;
            if (args.Targets.Count == 0)
            {
                foreach (Player p in Game.CurrentGame.AlivePlayers)
                {
                    if (p != source && v1.FastVerify(source, skill, cards, new List<Player>() { p }) != VerifierResult.Fail)
                    {
                        args.Targets.Add(p);
                        break;
                    }
                }
            }
            try
            {
                Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
            }
            catch (TriggerResultException)
            {
                //程序总是不应该执行到这里的
                Trace.Assert(false);
            }
        }
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

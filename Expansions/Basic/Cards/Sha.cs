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

        public override void Process(Player source, List<Player> dests, ICard card)
        {
            source[NumberOfShaUsed]++;
            base.Process(source, dests, card);
        }

        protected override void Process(Player source, Player dest, ICard card)
        {
            retrySha:
            GameEventArgs args = new GameEventArgs();
            args.Source = source;
            args.Targets = new List<Player>();
            args.Targets.Add(dest);
            args.Card = card;
            args.IntArg = 1;
            args.IntArg2 = 0;
            args.IntArg3 = 0;
            Game.CurrentGame.Emit(PlayerShaTargetShanModifier, args);
            int numberOfShanRequired = args.IntArg;
            bool cannotUseShan = args.IntArg2 == 1 ? true : false;
            if (args.IntArg3 == 0)
            {
                try
                {
                    Game.CurrentGame.Emit(PlayerShaTargetArmorModifier, args);
                }
                catch (TriggerResultException e)
                {
                    Trace.Assert(e.Status == TriggerResult.Fail);
                    return;
                }
            }
            bool cannotProvideShan = false;
            while (numberOfShanRequired > 0 && !cannotUseShan)
            {
                args.Source = dest;
                args.Targets = null;
                args.Card = new CompositeCard();
                args.Card.Type = new Shan();
                try
                {
                    Game.CurrentGame.Emit(GameEvent.PlayerRequireCard, args);
                }
                catch (TriggerResultException e)
                {
                    if (e.Status == TriggerResult.Success)
                    {
                        Game.CurrentGame.PlayerPlayedCard(dest, args.Card);
                        numberOfShanRequired--;
                        continue;
                    }
                }
                while (true)
                {
                    IUiProxy ui = Game.CurrentGame.UiProxies[dest];
                    SingleCardUsageVerifier v1 = new SingleCardUsageVerifier((c) => { return c.Type is Shan; });
                    ISkill skill;
                    List<Player> p;
                    List<Card> cards;
                    if (!ui.AskForCardUsage("Shan", v1, out skill, out cards, out p))
                    {
                        cannotProvideShan = true;
                        break;
                    }
                    if (!Game.CurrentGame.HandleCardUse(dest, skill, cards))
                    {
                        continue;
                    }
                    break;
                }
                if (cannotProvideShan)
                {
                    break;
                }
                numberOfShanRequired--;
            }
            if (cannotUseShan || numberOfShanRequired > 0)
            {
                Game.CurrentGame.DoDamage(source, dest, 1, ShaDamageElement, card);
            }
            else
            {
                Trace.TraceInformation("Successfully dodged");
                args = new GameEventArgs();
                args.Source = source;
                args.Targets = new List<Player>();
                args.Targets.Add(dest);
                args.Card = card;
                try
                {
                    Game.CurrentGame.Emit(PlayerShaTargetDodged, args);
                }
                catch (TriggerResultException e)
                {
                    if (e.Status == TriggerResult.Retry)
                    {
                        goto retrySha;
                    }
                    Trace.Assert(false);
                }
            }
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            if (targets != null && targets.Count > 0)
            {
                ShaEventArgs args = new ShaEventArgs();
                args.Source = source;
                args.RangeApproval = new List<bool>(targets.Count);
                args.TargetApproval = new List<bool>(targets.Count);
                foreach (Player t in targets)
                {
                    if (t == source)
                    {
                        return VerifierResult.Fail;
                    }
                    if (Game.CurrentGame.DistanceTo(source, t) <= source[PlayerAttribute.RangeAttack] + 1)
                    {
                        args.RangeApproval.Add(true);
                    }
                    else
                    {
                        args.RangeApproval.Add(false);
                    }
                    args.TargetApproval.Add(false);
                }
                if (source[NumberOfShaUsed] == 0)
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
                if (source[NumberOfShaUsed] == 0)
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
        public static string NumberOfShaUsed = "NumberOfShaUsed";
        /// <summary>
        /// 玩家使用杀的目标检测
        /// </summary>
        public static readonly GameEvent PlayerShaTargetValidation = new GameEvent("PlayerShaTargetValidation");
        /// <summary>
        /// 是否可以使用杀 
        /// </summary>
        public static readonly GameEvent PlayerNumberOfShaCheck = new GameEvent("PlayerNumberOfShaCheck");
        /// <summary>
        /// 杀目标需要闪的数目的修正
        /// </summary>
        public static readonly GameEvent PlayerShaTargetShanModifier = new GameEvent("PlayerShaTargetShanModifier");
        /// <summary>
        /// 杀被闪
        /// </summary>
        public static readonly GameEvent PlayerShaTargetDodged = new GameEvent("PlayerShaTargetDodged");
        /// <summary>
        /// 杀目标防具的闪的数目和杀的有效性修正
        /// </summary>
        public static readonly GameEvent PlayerShaTargetArmorModifier = new GameEvent("PlayerShaTargetArmorModifier");
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

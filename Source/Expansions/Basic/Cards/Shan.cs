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
    
    public class Shan : CardHandler
    {
        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard)
        {
        }

        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs eventArgs)
        {
            if (eventArgs != null)
            {
                eventArgs.ReadonlyCard[CardAttribute.Register(CardAttribute.TargetRequireTwoResponses + source.Id)]--;
            }
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            return VerifierResult.Fail;
        }

        public override CardCategory Category
        {
            get { return CardCategory.Basic; }
        }
    }

    public class ShaCancelling : Trigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (eventArgs.ReadonlyCard == null)
            {
                return;
            }
            if (!(eventArgs.ReadonlyCard.Type is Sha))
            {
                return;
            }
            Player source = eventArgs.Source;
            Player dest = eventArgs.Targets[0];
            ICard card = eventArgs.Card;
            List<Player> sourceList = new List<Player>() { source };
            GameEventArgs args = new GameEventArgs();
            Game.CurrentGame.Emit(PlayerShaTargetShanModifier, args);
            // this number is 0 for normal Sha/Shan. Lv Bu would like this to be 1
            eventArgs.ReadonlyCard[CardAttribute.Register(CardAttribute.TargetRequireTwoResponses + dest.Id)]++;
            int numberOfShanRequired = eventArgs.ReadonlyCard[CardAttribute.Register(CardAttribute.TargetRequireTwoResponses + dest.Id)];
            bool cannotUseShan = eventArgs.ReadonlyCard[CannotProvideShan] == 1 ? true : false;
            bool cannotProvideShan = false;
            while (numberOfShanRequired > 0 && !cannotUseShan)
            {
                args.Source = dest;
                args.Targets = sourceList;
                args.Card = new CompositeCard();
                args.Card.Type = new Shan();
                args.ReadonlyCard = eventArgs.ReadonlyCard;
                try
                {
                    Game.CurrentGame.Emit(GameEvent.PlayerRequireCard, args);
                }
                catch (TriggerResultException e)
                {
                    if (e.Status == TriggerResult.Success)
                    {
                        GameEventArgs arg = new GameEventArgs();
                        arg.Source = dest;
                        arg.Targets = sourceList;
                        arg.Skill = args.Skill;
                        arg.Cards = args.Cards;
                        arg.InResponseTo = eventArgs;
                        Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, arg);
#if SB_FAQ
                        numberOfShanRequired --;
#else
                        numberOfShanRequired = eventArgs.ReadonlyCard[CardAttribute.Register(CardAttribute.TargetRequireTwoResponses + dest.Id)];
#endif
                        continue;
                    }
                }
                while (true)
                {
                    IUiProxy ui = Game.CurrentGame.UiProxies[dest];
                    SingleCardUsageVerifier v1 = new SingleCardUsageVerifier((c) => { return c.Type is Shan; }, true);
                    ISkill skill;
                    List<Player> p;
                    List<Card> cards;
                    if (!ui.AskForCardUsage(new CardUsagePrompt("Sha.Shan", source), v1, out skill, out cards, out p))
                    {
                        cannotProvideShan = true;
                        break;
                    }
                    try
                    {
                        GameEventArgs arg = new GameEventArgs();
                        arg.Source = dest;
                        arg.Targets = sourceList;
                        arg.Skill = skill;
                        arg.Cards = cards;
                        arg.InResponseTo = eventArgs;
                        Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, arg);
                    }
                    catch (TriggerResultException e)
                    {
                        Trace.Assert(e.Status == TriggerResult.Retry);
                        continue;
                    }
                    break;
                }
                if (cannotProvideShan)
                {
                    break;
                }
#if SB_FAQ
                numberOfShanRequired --;
#else
                numberOfShanRequired = eventArgs.ReadonlyCard[CardAttribute.Register(CardAttribute.TargetRequireTwoResponses + dest.Id)];
#endif
            }
            if (cannotUseShan ||
#if SB_FAQ
                eventArgs.ReadonlyCard[CardAttribute.Register(CardAttribute.TargetRequireTwoResponses + dest.Id)] > 0
#else
                numberOfShanRequired > 0
#endif
                ) return;
            Trace.TraceInformation("Successfully dodged");
            args = new GameEventArgs();
            args.Source = source;
            args.Targets = new List<Player>();
            args.Targets.Add(dest);
            args.Card = card;
            args.ReadonlyCard = eventArgs.ReadonlyCard;
            try
            {
                Game.CurrentGame.Emit(PlayerShaTargetDodged, args);
            }
            catch (TriggerResultException)
            {
                Trace.Assert(false);
            }

            throw new TriggerResultException(TriggerResult.End);
        }
        /// <summary>
        /// 杀目标的修正
        /// </summary>
        public static readonly GameEvent PlayerShaTargetShanModifier = new GameEvent("PlayerShaTargetShanModifier");
        /// <summary>
        /// 杀被闪
        /// </summary>
        public static readonly GameEvent PlayerShaTargetDodged = new GameEvent("PlayerShaTargetDodged");
        public static readonly CardAttribute CannotProvideShan = CardAttribute.Register("CannotProvideShan");
    }
}

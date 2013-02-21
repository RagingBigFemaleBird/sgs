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
    public abstract class Aoe : CardHandler
    {
        [NonSerialized]
        private SingleCardUsageVerifier responseCardVerifier;

        protected SingleCardUsageVerifier ResponseCardVerifier
        {
            get { return responseCardVerifier; }
            set { responseCardVerifier = value; }
        }

        protected abstract string UsagePromptString { get; }

        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
        {
            SingleCardUsageVerifier v1 = responseCardVerifier;
            List<Player> sourceList = new List<Player>();
            sourceList.Add(source);
            GameEventArgs args = new GameEventArgs();
            args.Source = dest;
            args.Targets = sourceList;
            args.Card = new CompositeCard();
            args.Card.Type = RequiredCard();
            args.ReadonlyCard = readonlyCard;
            try
            {
                Game.CurrentGame.Emit(GameEvent.PlayerRequireCard, args);
            }
            catch (TriggerResultException e)
            {
                if (e.Status == TriggerResult.Success)
                {
                    Game.CurrentGame.HandleCardPlay(dest, args.Skill, args.Cards, sourceList);
                    return;
                }
            }
            while (true)
            {
                IUiProxy ui = Game.CurrentGame.UiProxies[dest];
                ISkill skill;
                List<Player> p;
                List<Card> cards;
                if (!ui.AskForCardUsage(new CardUsagePrompt(UsagePromptString, source),
                                                      v1, out skill, out cards, out p))
                {
                    Trace.TraceInformation("Player {0} Invalid answer", dest);
                    Game.CurrentGame.DoDamage(source.IsDead ? null : source, dest, 1, DamageElement.None, card, readonlyCard);
                }
                else
                {
                    if (!Game.CurrentGame.HandleCardPlay(dest, skill, cards, sourceList))
                    {
                        continue;
                    }
                    Trace.TraceInformation("Player {0} Responded. ", dest.Id);
                }
                break;
            }
        }

        protected abstract CardHandler RequiredCard();

        public override List<Player> ActualTargets(Player source, List<Player> dests, ICard card)
        {
            var targets = new List<Player>(Game.CurrentGame.AlivePlayers);
            targets.Remove(source);
            var backup = new List<Player>(targets);
            foreach (var t in backup)
            {
                if (!Game.CurrentGame.PlayerCanBeTargeted(source, new List<Player>() { t }, card))
                {
                    targets.Remove(t);
                }
            }
            return targets;
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            if (targets != null && targets.Count >= 1)
            {
                return VerifierResult.Fail;
            }
            if (ActualTargets(source, targets, card).Count == 0) return VerifierResult.Fail;
            return VerifierResult.Success;
        }

        public override CardCategory Category
        {
            get { return CardCategory.ImmediateTool; }
        }
    }
}

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

        private SingleCardUsageVerifier responseCardVerifier;

        protected SingleCardUsageVerifier ResponseCardVerifier
        {
            get { return responseCardVerifier; }
            set { responseCardVerifier = value; }
        }

        protected abstract string UsagePromptString { get; }

        protected override void Process(Player source, Player dest, ICard card)
        {
            throw new NotImplementedException();
        }

        public override void Process(Player source, List<Player> dests, ICard c)
        {
            Trace.Assert(dests == null || dests.Count == 0);
            NotifyCardUse(source, dests, new List<Player>(), c);
            SingleCardUsageVerifier v1 = responseCardVerifier;
            List<Player> toProcess = new List<Player>(Game.CurrentGame.AlivePlayers);
            toProcess.Remove(source);
            Game.CurrentGame.SortByOrderOfComputation(source, toProcess);
            foreach (var player in toProcess)
            {
                Player current = player;
                Player src = source;
                if (!PlayerIsCardTargetCheck(ref src, ref current, c))
                {
                    continue;
                }
                List<Player> sourceList = new List<Player>();
                sourceList.Add(src);
                GameEventArgs args = new GameEventArgs();
                args.Source = current;
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
                        Game.CurrentGame.HandleCardPlay(current, null, args.Cards, sourceList);
                        continue;
                    }
                }
                while (true)
                {
                    IUiProxy ui = Game.CurrentGame.UiProxies[current];
                    ISkill skill;
                    List<Player> p;
                    List<Card> cards;
                    if (!ui.AskForCardUsage(new CardUsagePrompt(UsagePromptString, src),
                                                          v1, out skill, out cards, out p))
                    {
                        Trace.TraceInformation("Player {0} Invalid answer", current);
                        Game.CurrentGame.DoDamage(src, current, 1, DamageElement.None, c);
                    }
                    else
                    {
                        if (!Game.CurrentGame.HandleCardPlay(current, skill, cards, sourceList))
                        {
                            continue;
                        }
                        Trace.TraceInformation("Player {0} Responded. ", current.Id);
                    }
                    break;
                }

            }
        }

        protected override List<Player> LogTargetsModifier(Player source, List<Player> dests)
        {
            var z = new List<Player>(Game.CurrentGame.AlivePlayers);
            z.Remove(source);
            return z;
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            if (targets != null && targets.Count >= 1)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public override CardCategory Category
        {
            get { return CardCategory.ImmediateTool; }
        }
    }
}

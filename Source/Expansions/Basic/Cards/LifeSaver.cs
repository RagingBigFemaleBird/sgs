using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;
using System.Diagnostics;

namespace Sanguosha.Expansions.Basic.Cards
{
    public class LifeSaver : CardHandler
    {
        public override CardCategory Category
        {
            get { return CardCategory.Basic; }
        }

        protected override void Process(Core.Players.Player source, Core.Players.Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
        {
            throw new NotImplementedException();
        }

        protected override Core.UI.VerifierResult Verify(Core.Players.Player source, ICard card, List<Core.Players.Player> targets)
        {
            throw new NotImplementedException();
        }
    }

    public class LifeSaverVerifier : CardUsageVerifier
    {
        public Player DyingPlayer { get; set; }
        public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
        {
            List<Player> l = new List<Player>();
            if (players != null) l.AddRange(players);
            l.Add(DyingPlayer);
            ICard card;
            if (skill != null)
            {
                if (skill is CardTransformSkill)
                {
                    CompositeCard c;
                    var result = (skill as CardTransformSkill).TryTransform(cards, null, out c);
                    if (result != VerifierResult.Success)
                    {
                        return result;
                    }
                    card = c;
                }
                else
                {
                    return VerifierResult.Fail;
                }
            }
            else
            {
                if (cards == null || cards.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                card = cards[0];
            }
            if (!(card.Type is LifeSaver))
            {
                return VerifierResult.Fail;
            }
            return card.Type.Verify(source, skill, cards, l);
        }

        public override IList<CardHandler> AcceptableCardTypes
        {
            get { return new List<CardHandler>() { new LifeSaver() }; }
        }
    }

    public class PlayerDying : Trigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            Player target = eventArgs.Source;
            if (target.Health > 0) return;
            LifeSaverVerifier v = new LifeSaverVerifier();
            v.DyingPlayer = target;
            Player p = eventArgs.Targets[0];
            if (!Game.CurrentGame.PlayerCanUseCard(p, new Card() { Place = new DeckPlace(p, DeckType.None), Type = new Tao() })) return;
            if (p.IsDead) return;
            while (true)
            {
                ISkill skill;
                List<Card> cards;
                List<Player> players;
                if (Game.CurrentGame.UiProxies[p].AskForCardUsage(new CardUsagePrompt("SaveALife", target, 1 - target.Health), v, out skill, out cards, out players))
                {
                    try
                    {
                        GameEventArgs args = new GameEventArgs();
                        args.Source = p;
                        args.Skill = skill;
                        args.Cards = cards;
                        Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
                    }
                    catch (TriggerResultException e)
                    {
                        Trace.Assert(e.Status == TriggerResult.Retry);
                        continue;
                    }
                    if (target.IsDead || target.Health > 0) break;
                }
                else break;
            }
        }
    }
}

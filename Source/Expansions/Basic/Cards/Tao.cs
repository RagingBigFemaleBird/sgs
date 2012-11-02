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
    public class Tao : CardHandler
    {
        protected override void Process(Player source, Player dest, ICard card)
        {
            throw new NotImplementedException();
        }

        public override void Process(Player source, List<Player> dests, ICard card)
        {
            Trace.Assert(dests == null || dests.Count == 0);
            NotifyCardUse(source, new List<Player>() {source}, null, card);
            if (!PlayerIsCardTargetCheck(ref source, ref source, card))
            {
                return;
            }
            Game.CurrentGame.RecoverHealth(source, source, 1);
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            if (Game.CurrentGame.IsDying.Count == 0 && targets != null && targets.Count >= 1)
            {
                return VerifierResult.Fail;
            }
            if (Game.CurrentGame.IsDying.Count > 0 && (targets == null || targets.Count != 1))
            {
                return VerifierResult.Fail;
            }
            Player p;
            if (Game.CurrentGame.IsDying.Count == 0)
            {
                p = source;
            }
            else
            {
                p = targets[0];
            }
            if (p.Health >= p.MaxHealth)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public override CardCategory Category
        {
            get { return CardCategory.Basic; }
        }
    }

    public class TaoJiuVerifier : CardUsageVerifier
    {
        public Player DyingPlayer { get; set; }
        public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
        {
            List<Player> l = new List<Player>();
            if (players != null) l.AddRange(players);
            l.Add(DyingPlayer);
            return (new Tao()).Verify(source, skill, cards, l);
        }

        public override IList<CardHandler> AcceptableCardType
        {
            get { return new List<CardHandler>() { new Tao() }; }
        }
    }

    public class PlayerDying : Trigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            Player target = eventArgs.Targets[0];
            if (target.Health > 0) return;
            Game.CurrentGame.IsDying.Push(target);
            List<Player> toAsk = new List<Player>(Game.CurrentGame.AlivePlayers);
            Game.CurrentGame.SortByOrderOfComputation(Game.CurrentGame.CurrentPlayer, toAsk);
            TaoJiuVerifier v = new TaoJiuVerifier();
            v.DyingPlayer = target;
            foreach (Player p in toAsk)
            {
                while (true)
                {
                    ISkill skill;
                    List<Card> cards;
                    List<Player> players;
                    if (Game.CurrentGame.UiProxies[p].AskForCardUsage(new CardUsagePrompt("SaveALife", target), v, out skill, out cards, out players))
                    {
                        if (!Game.CurrentGame.HandleCardPlay(p, skill, cards, players))
                        {
                            continue;
                        }
                        Game.CurrentGame.RecoverHealth(p, target, 1);
                        if (target.Health > 0)
                        {
                            Trace.Assert(target == Game.CurrentGame.IsDying.Pop());
                            return;
                        }
                    }
                    break;
                }
            }
            Trace.TraceInformation("Player {0} dead", target.Id);
            target.IsDead = true;
            Game.CurrentGame.Emit(GameEvent.PlayerIsDead, eventArgs);
            Game.CurrentGame.SyncCardsAll(Game.CurrentGame.Decks[target, DeckType.Hand]);
            CardsMovement move = new CardsMovement();
            move.cards = new List<Card>();
            move.cards.AddRange(Game.CurrentGame.Decks[target, DeckType.Hand]);
            move.cards.AddRange(Game.CurrentGame.Decks[target, DeckType.Equipment]);
            move.cards.AddRange(Game.CurrentGame.Decks[target, DeckType.DelayedTools]);
            move.to = new DeckPlace(null, DeckType.Discard);
            Game.CurrentGame.MoveCards(move, null);
            Trace.Assert(target == Game.CurrentGame.IsDying.Pop());

        }
    }

}

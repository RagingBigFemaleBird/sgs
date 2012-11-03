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
    public class JieDaoShaRen : CardHandler
    {
        protected override void Process(Player source, Player dest, ICard card)
        {
            throw new NotImplementedException();
        }

        private class JieDaoShaRenVerifier : CardUsageVerifier
        {
            public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if (!Game.CurrentGame.AllAlive(players))
                {
                    return VerifierResult.Fail;
                }
                if (players == null)
                {
                    players = new List<Player>();
                }
                List<Player> newList = new List<Player>(players);
                if (!newList.Contains(target))
                {
                    newList.Add(target);
                }
                return (new Sha()).Verify(source, skill, cards, newList);
            }

            public override IList<CardHandler> AcceptableCardType
            {
                get { return new List<CardHandler>() {new Sha()}; }
            }

            Player target;

            public JieDaoShaRenVerifier(Player t)
            {
                target = t;
            }
        }

        public override void Process(Player source, List<Player> dests, ICard card)
        {
            Trace.Assert(dests.Count == 2);
            Player initiator = dests[0];
            if (PlayerIsCardTargetCheck(ref source, ref initiator, card))
            {
                ISkill skill;
                List<Card> cards;
                List<Player> players;
                if (!dests[1].IsDead && Game.CurrentGame.UiProxies[initiator].AskForCardUsage(new CardUsagePrompt("JieDaoShaRen", dests[1]), new JieDaoShaRenVerifier(dests[1]), out skill, out cards, out players))
                {
                    GameEventArgs args = new GameEventArgs();
                    args.Source = initiator;
                    args.Targets = players;
                    args.Targets.Add(dests[1]);
                    args.Skill = skill;
                    args.Cards = cards;
                    Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
                }
                else
                {
                    if (source.IsDead) return;
                    Card theWeapon = null;
                    foreach (Card c in Game.CurrentGame.Decks[initiator, DeckType.Equipment])
                    {
                        if (c.Type is Weapon)
                        {
                            theWeapon = c;
                            break;
                        }
                    }
                    if (theWeapon != null)
                    {
                        Game.CurrentGame.HandleCardTransferToHand(initiator, source, new List<Card>() { theWeapon });
                    }
                }
            }
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            if (targets == null || targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (targets.Count > 2)
            {
                return VerifierResult.Fail;
            }
            bool hasWeapon = false;
            foreach (var c in Game.CurrentGame.Decks[targets[0], DeckType.Equipment])
            {
                if (c.Type is Weapon)
                {
                    hasWeapon = true;
                }
            }
            if (!hasWeapon)
            {
                return VerifierResult.Fail;
            }
            if (targets.Count == 2)
            {
                if (!Game.CurrentGame.PlayerCanBeTargeted(targets[0], new List<Player>() { targets[1] }, new CompositeCard() { Type = new Sha() }))
                {
                    return VerifierResult.Fail;
                }
                if ((new Sha()).ShaVerifyForJieDaoShaRenOnly(targets[0], null, new List<Player>() { targets[1] }) != VerifierResult.Success)
                {
                    return VerifierResult.Fail;
                }
            }
            return VerifierResult.Success;
        }

        public override CardCategory Category
        {
            get { return CardCategory.ImmediateTool; }
        }
    }
}

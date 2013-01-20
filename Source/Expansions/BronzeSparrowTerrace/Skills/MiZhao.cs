using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.BronzeSparrowTerrace.Skills
{
    /// <summary>
    /// 密诏-出牌阶段，你可以将所有手牌交给一名其他角色，令该角色与你选择的另一名角色拼点，视为点数大的角色对点数小的角色使用一张【杀】（无距离限制）。每阶段限一次。
    /// </summary>
    public class MiZhao : AutoVerifiedActiveSkill
    {
        class MiZhaoVerifier : CardsAndTargetsVerifier
        {
            Player target;
            public MiZhaoVerifier(Player target)
            {
                this.target = target;
                MaxCards = 0;
                MaxPlayers = 1;
                MinPlayers = 1;
            }

            protected override bool VerifyPlayer(Player source, Player player)
            {
                return player != target && player.HandCards().Count > 0;
            }
        }

        public MiZhao()
        {
            MaxPlayers = 1;
            MinPlayers = 1;
            Helper.NoCardReveal = true;
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return true;
        }

        protected override bool VerifyCard(Player source, Card card)
        {
            return card.Place.DeckType == DeckType.Hand;
        }

        protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            if (source[MiZhaoUsed] != 0 || source.HandCards().Count == 0) return false;
            if (cards != null && cards.Count < source.HandCards().Count) return null;
            return true;
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[MiZhaoUsed] = 1;
            Game.CurrentGame.HandleCardTransferToHand(Owner, arg.Targets[0], arg.Cards);
            List<Player> alivePlayers = Game.CurrentGame.AlivePlayers;
            if (!alivePlayers.Any(p => p.HandCards().Count > 0 && p != arg.Targets[0])) return true;
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (!Owner.AskForCardUsage(new CardUsagePrompt("MiZhao", arg.Targets[0]), new MiZhaoVerifier(arg.Targets[0]), out skill, out cards, out players))
            {
                players = new List<Player>();
                foreach (Player p in alivePlayers)
                {
                    if (p.HandCards().Count > 0 && p != arg.Targets[0])
                    {
                        players.Add(p);
                        break;
                    }
                }
            }
            Player pindianTarget = players[0];
            Card card1, card2;
            Game.CurrentGame.PinDianReturnCards(arg.Targets[0], pindianTarget, out card1, out card2, this);
            Game.CurrentGame.EnterAtomicContext();
            Game.CurrentGame.PlaceIntoDiscard(arg.Targets[0], new List<Card>() { card1 });
            Game.CurrentGame.PlaceIntoDiscard(pindianTarget, new List<Card>() { card2 });
            Game.CurrentGame.ExitAtomicContext();
            if (card1.Rank == card2.Rank) return true;
            Player winer, loser;
            if (card1.Rank > card2.Rank) { winer = arg.Targets[0]; loser = pindianTarget; }
            else { winer = pindianTarget; loser = arg.Targets[0]; }
            if (!Game.CurrentGame.PlayerCanBeTargeted(winer, new List<Player>() { loser }, new CompositeCard() { Type = new Sha() })) return true;
            GameEventArgs args = new GameEventArgs();
            args.Source = winer;
            args.Targets = new List<Player>() { loser };
            args.Skill = new CardWrapper(winer, new RegularSha());
            Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
            return true;
        }

        private static PlayerAttribute MiZhaoUsed = PlayerAttribute.Register("MiZhaoUsed", true);
    }
}

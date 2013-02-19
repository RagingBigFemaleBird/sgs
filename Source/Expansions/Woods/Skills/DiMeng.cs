using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 缔盟-出牌阶段，你可以选择两名其他角色，你弃置等同于这两名角色手牌数差的牌，然后交换他们的手牌。每阶段限一次。
    /// </summary>
    public class DiMeng : AutoVerifiedActiveSkill
    {
        public override bool Commit(GameEventArgs arg)
        {
            Owner[DiMengUsed] = 1;
            List<Card> cards = arg.Cards;
            if (cards.Count > 0)
            {
                Game.CurrentGame.HandleCardDiscard(Owner, cards);
            }
            Player src1 = arg.Targets[0];
            Player src2 = arg.Targets[1];
            StagingDeckType DiMengDeck = new StagingDeckType("DiMeng");
            Game.CurrentGame.EnterAtomicContext();
            CardsMovement move = new CardsMovement();
            move.Helper.IsFakedMove = true;
            cards = new List<Card>(Game.CurrentGame.Decks[src1, DeckType.Hand]);
            move.Cards = new List<Card>(cards);
            move.To = new DeckPlace(src1, DiMengDeck);
            Game.CurrentGame.MoveCards(move);
            Game.CurrentGame.PlayerLostCard(src1, cards);

            cards = new List<Card>(Game.CurrentGame.Decks[src2, DeckType.Hand]);
            move.Cards = new List<Card>(cards);
            move.To = new DeckPlace(src2, DiMengDeck);
            Game.CurrentGame.MoveCards(move);
            Game.CurrentGame.PlayerLostCard(src2, cards);
            Game.CurrentGame.ExitAtomicContext();

            move.Helper.IsFakedMove = false;
            Game.CurrentGame.SyncImmutableCards(src1, Game.CurrentGame.Decks[src2, DiMengDeck]);
            Game.CurrentGame.SyncImmutableCards(src2, Game.CurrentGame.Decks[src1, DiMengDeck]);
            Game.CurrentGame.EnterAtomicContext();
            cards = new List<Card>(Game.CurrentGame.Decks[src2, DiMengDeck]);
            move.Cards = new List<Card>(cards);
            move.To = new DeckPlace(src1, DeckType.Hand);
            Game.CurrentGame.MoveCards(move);
            Game.CurrentGame.PlayerAcquiredCard(src1, cards);

            cards = new List<Card>(Game.CurrentGame.Decks[src1, DiMengDeck]);
            move.Cards = new List<Card>(cards);
            move.To = new DeckPlace(src2, DeckType.Hand);
            Game.CurrentGame.MoveCards(move);
            Game.CurrentGame.PlayerAcquiredCard(src2, cards);
            Game.CurrentGame.ExitAtomicContext();
            return true;
        }

        public static PlayerAttribute DiMengUsed = PlayerAttribute.Register("DiMengUsed", true);

        public DiMeng()
        {
            MaxPlayers = 2;
            MinPlayers = 2;
            Discarding = true;
        }

        protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            if (source[DiMengUsed] != 0) return false;
            if (players != null && players.Count < 2)
            {
                if (cards != null && cards.Count > 0) return false;
                return null;
            }
            int diff = Math.Abs(Game.CurrentGame.Decks[players[0], DeckType.Hand].Count - Game.CurrentGame.Decks[players[1], DeckType.Hand].Count);
            int count;
            if (cards == null) count = 0;
            else count = cards.Count;
            if (count > diff) return false;
            if (count < diff) return null;
            return true;
        }

        protected override bool VerifyCard(Player source, Card card)
        {
            return true;
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return source != player;
        }
    }
}

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

namespace Sanguosha.Expansions.OverKnightFame11.Skills
{
    /// <summary>
    /// 甘露-出牌阶段，你可以选择两名角色，交换他们装备区里的所有牌。以此法交换的装备数差不能超过X(X为你已损失体力值)。每回合限一次。
    /// </summary>
    public class GanLu : AutoVerifiedActiveSkill
    {
        public override bool Commit(GameEventArgs arg)
        {
            Owner[GanLuUsed] = 1;
            List<Card> cards;
            Player src1 = arg.Targets[0];
            Player src2 = arg.Targets[1];
            StagingDeckType GanLuDeck = new StagingDeckType("GanLu");
            Game.CurrentGame.EnterAtomicContext();
            CardsMovement move = new CardsMovement();
            move.Helper.IsFakedMove = true;
            cards = new List<Card>(src1.Equipments());
            move.Cards = new List<Card>(cards);
            move.To = new DeckPlace(src1, GanLuDeck);
            Game.CurrentGame.MoveCards(move);
            Game.CurrentGame.PlayerLostCard(src1, cards);

            cards = new List<Card>(src2.Equipments());
            move.Cards = new List<Card>(cards);
            move.To = new DeckPlace(src2, GanLuDeck);
            Game.CurrentGame.MoveCards(move);
            Game.CurrentGame.PlayerLostCard(src2, cards);
            Game.CurrentGame.ExitAtomicContext();

            Game.CurrentGame.EnterAtomicContext();
            move.Helper.IsFakedMove = false;
            cards = new List<Card>(Game.CurrentGame.Decks[src2, GanLuDeck]);
            move.Cards = new List<Card>(cards);
            move.To = new DeckPlace(src1, DeckType.Equipment);
            Game.CurrentGame.MoveCards(move);
            Game.CurrentGame.PlayerAcquiredCard(src1, cards);

            cards = new List<Card>(Game.CurrentGame.Decks[src1, GanLuDeck]);
            move.Cards = new List<Card>(cards);
            move.To = new DeckPlace(src2, DeckType.Equipment);
            Game.CurrentGame.MoveCards(move);
            Game.CurrentGame.PlayerAcquiredCard(src2, cards);
            Game.CurrentGame.ExitAtomicContext();
            return true;
        }

        public static PlayerAttribute GanLuUsed = PlayerAttribute.Register("GanLuUsed", true);

        public GanLu()
        {
            MinCards = 0;
            MaxCards = 0;
            MaxPlayers = 2;
            MinPlayers = 2;
        }

        protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            if (source[GanLuUsed] != 0) return false;
            if (players != null && players.Count == 2)
            {
                int diff = Math.Abs(players[0].Equipments().Count - players[1].Equipments().Count);
                if (diff > source.LostHealth) return false;
            }
            return true;
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return true;
        }
        protected override bool VerifyCard(Player source, Card card)
        {
            return true;
        }

    }
}

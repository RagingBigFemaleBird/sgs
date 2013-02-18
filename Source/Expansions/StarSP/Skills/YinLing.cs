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
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.StarSP.Skills
{
    /// <summary>
    /// 银铃–出牌阶段，你可以弃置一张黑色牌并指定一名其他角色。若如此做，你获得其一张牌并置于你的武将牌上，称为“锦”。（数量最多为四）
    /// </summary>
    public class YinLing : AutoVerifiedActiveSkill
    {
        public YinLing()
        {
            MaxCards = 1;
            MinCards = 1;
            MaxPlayers = 1;
            MinPlayers = 1;
            Discarding = true;
            DeckCleanup.Add(JinDeck);
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return player != source && player.HandCards().Count + player.Equipments().Count > 0;
        }

        protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            if (Game.CurrentGame.Decks[source, JinDeck].Count >= 4)
                return false;
            return true;
        }

        protected override bool VerifyCard(Player source, Card card)
        {
            return card.SuitColor == SuitColorType.Black;
        }

        public override bool Commit(GameEventArgs arg)
        {
            Game.CurrentGame.HandleCardDiscard(Owner, arg.Cards);
            if (arg.Targets[0].HandCards().Count + arg.Targets[0].Equipments().Count == 0) return true;
            List<DeckPlace> souceDeck = new List<DeckPlace>();
            souceDeck.Add(new DeckPlace(arg.Targets[0], DeckType.Hand));
            souceDeck.Add(new DeckPlace(arg.Targets[0], DeckType.Equipment));
            List<string> resultDeckNames = new List<string>() { "YinLing" };
            List<int> resultDeckMaximuns = new List<int>() { 1 };
            List<List<Card>> answer;
            if (!Owner.AskForCardChoice(new CardChoicePrompt("YinLing", arg.Targets[0], Owner), souceDeck, resultDeckNames, resultDeckMaximuns, new RequireOneCardChoiceVerifier(), out answer))
            {
                answer = new List<List<Card>>();
                answer.Add(new List<Card>() { arg.Targets[0].HandCards().Concat(arg.Targets[0].Equipments()).First() });
            }
            Game.CurrentGame.HandleCardTransfer(arg.Targets[0], Owner, JinDeck, answer[0], HeroTag);
            return true;
        }

        public static PrivateDeckType JinDeck = new PrivateDeckType("Jin", true);
    }
}

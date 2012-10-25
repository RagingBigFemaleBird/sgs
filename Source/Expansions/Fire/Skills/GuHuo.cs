using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using System.Diagnostics;

namespace Sanguosha.Expansions.Fire.Skills
{
    /// <summary>
    /// 蛊惑–当你需要使用或打出一张基本牌或非延时类锦囊牌时，你可以声明并将一张手牌扣于桌上。若无人质疑，则该牌按你所述之牌来用。若有人质疑则亮出验明：若为真，质疑者各失去一点体力；若为假，质疑者各摸一张牌。除非被质疑的牌的花色为红桃且为真（仍然可用），否则无论真假，该牌都作废，弃置之。
    /// </summary>
    class GuHuo : CardTransformSkill
    {
        public override UiHelper Helper
        {
            get
            {
                return new UiHelper() { isGuHuo = true };
            }
        }
        public override VerifierResult TryTransform(List<Card> cards, object arg, out CompositeCard card)
        {
            card = null;
            if (cards == null || cards.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (cards != null && cards.Count != 1)
            {
                return VerifierResult.Fail;
            }

            card = new CompositeCard();
            card.Subcards = null;
            card.Type = cards[0].AddtionalGenericType;
            return VerifierResult.Success;
        }

        protected override bool DoTransformSideEffect(CompositeCard card, object arg, List<Player> targets)
        {
            CardsMovement move = new CardsMovement();
            Trace.Assert(card.Subcards.Count == 1);
            move.cards = new List<Card>();
            move.cards.AddRange(card.Subcards);
            move.to = new DeckPlace(null, DeckType.GuHuo);
            Game.CurrentGame.MoveCards(move, null);
            Game.CurrentGame.PlayerLostCard(Owner, move.cards);
            Player player = Game.CurrentGame.NextPlayer(Owner);
            Dictionary<Player, int> believe = new Dictionary<Player,int>();
            while (player != Owner)
            {
                int answer = 1;
                if (!Game.CurrentGame.UiProxies[player].AskForMultipleChoice(new MultipleChoicePrompt("GuHuo", Owner, (move.cards[0].AddtionalGenericType).CardType), Prompt.YesNoChoices, out answer))
                {
                    //override default answer to no
                    answer = 1;
                }
                believe.Add(player, answer);
                player = Game.CurrentGame.NextPlayer(player);
            }
            Game.CurrentGame.SyncCardAll(Game.CurrentGame.Decks[null, DeckType.GuHuo][0]);
            bool guhuoSucceed = true;
            foreach (var v in believe)
            {
                if (v.Value == 0)
                {
                    guhuoSucceed = false;
                    break;
                }
            }
            if (!guhuoSucceed)
            {
                if (Game.CurrentGame.Decks[null, DeckType.GuHuo][0].Type.GetType().IsAssignableFrom(Game.CurrentGame.Decks[null, DeckType.GuHuo][0].AddtionalGenericType.GetType()))
                {
                    player = Game.CurrentGame.NextPlayer(Owner);
                    while (player != Owner)
                    {
                        if (believe[player] == 0)
                        {
                            Game.CurrentGame.LoseHealth(player, 1);
                        }
                        player = Game.CurrentGame.NextPlayer(player);
                    }
                }
                if (Game.CurrentGame.Decks[null, DeckType.GuHuo][0].Suit != SuitType.Heart)
                {
                    move = new CardsMovement();
                    move.cards = new List<Card>();
                    move.cards.AddRange(Game.CurrentGame.Decks[null, DeckType.GuHuo]);
                    move.to = new DeckPlace(null, DeckType.Discard);
                    Game.CurrentGame.MoveCards(move, null);
                    return false;
                }
            }
            card.Subcards = new List<Card>();
            card.Subcards.Add(Game.CurrentGame.Decks[null, DeckType.GuHuo][0]);
            return true;
        }
    }
}

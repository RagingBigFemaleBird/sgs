using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;

namespace Sanguosha.Core.Cards
{
    public abstract class CardHandler
    {
        Dictionary<DeckPlace, List<Card>> cardsOnHold;

        /// <summary>
        /// 临时将卡牌提出，verify时使用，第二次调用将会摧毁第一次调用时临时区域的所有卡牌
        /// </summary>
        /// <param name="cards">卡牌</param>
        public virtual void HoldInTemp(List<Card> cards)
        {
            cardsOnHold = new Dictionary<DeckPlace, List<Card>>();
            foreach (Card c in cards)
            {
                if (!cardsOnHold.ContainsKey(c.Place))
                {
                    cardsOnHold.Add(c.Place, new List<Card>(Game.CurrentGame.Decks[c.Place]));
                }
            }
        }

        /// <summary>
        /// 回复临时区域的卡牌到原来位置
        /// </summary>
        public virtual void ReleaseHoldInTemp()
        {
            foreach (DeckPlace p in cardsOnHold.Keys)
            {
                Game.CurrentGame.Decks[p] = new List<Card>(cardsOnHold[p]);
            }
            cardsOnHold = null;
        }

        public virtual void Process(Player source, List<Player> dests)
        {
            foreach (var player in dests)
            {
                Process(source, player);
            }
        }

        protected abstract void Process(Player source, Player dest);

        public abstract VerifierResult Verify(ISkill skill, List<Card> cards, List<Player> players);

        public string CardType
        {
            get { return this.GetType().ToString(); }
        }
    }

}

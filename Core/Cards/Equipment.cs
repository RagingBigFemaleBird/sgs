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

namespace Sanguosha.Core.Cards
{
    public abstract class Equipment : CardHandler
    {
        /// <summary>
        /// 注册武器应有的trigger到玩家
        /// </summary>
        /// <param name="p"></param>
        public abstract void RegisterTriggers(Player p);
        /// <summary>
        /// 从玩家注销武器应有的trigger
        /// </summary>
        /// <param name="p"></param>
        public abstract void UnregisterTriggers(Player p);

        /// <summary>
        /// 给某个玩家穿装备
        /// </summary>
        /// <param name="p"></param>
        /// <param name="card"></param>
        public void Install(Player p, Card card)
        {
            CardsMovement attachMove = new CardsMovement();
            attachMove.cards = new List<Card>();
            attachMove.cards.Add(card);
            attachMove.to = new DeckPlace(p, DeckType.Equipment);
            foreach (Card c in Game.CurrentGame.Decks[p, DeckType.Equipment])
            {
                if (CardCategoryManager.IsCardCategory(c.Type.Category, this.Category))
                {
                    CardsMovement detachMove = new CardsMovement();
                    detachMove.cards = new List<Card>();
                    detachMove.cards.Add(c);
                    detachMove.to = new DeckPlace(null, DeckType.Discard);

                    List<CardsMovement> l = new List<CardsMovement>();
                    l.Add(detachMove);
                    l.Add(attachMove);
                    Equipment e = (Equipment)c.Type;
                    Trace.Assert(e != null);
                    e.UnregisterTriggers(p);
                    RegisterTriggers(p);
                    Game.CurrentGame.MoveCards(l, null);
                    return;
                }
            }

            RegisterTriggers(p);
            
            Game.CurrentGame.MoveCards(attachMove, null);
            return;
        }

        public override void Process(Player source, List<Player> dests, ICard card)
        {
            Trace.Assert(dests == null || dests.Count == 0);
            Trace.Assert(card is Card);
            Card c = (Card)card;
            Install(source, c);
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            if (targets == null || targets.Count == 0)
            {
                return VerifierResult.Success;
            }
            return VerifierResult.Fail;
        }
    }
}

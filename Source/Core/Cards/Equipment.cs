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
        /// 注册装备应有的trigger到玩家
        /// </summary>
        /// <param name="p"></param>
        protected virtual void RegisterEquipmentTriggers(Player p)
        {
        }
        /// <summary>
        /// 从玩家注销装备应有的trigger
        /// </summary>
        /// <param name="p"></param>
        protected virtual void UnregisterEquipmentTriggers(Player p)
        {
        }

        public virtual void RegisterTriggers(Player p)
        {
            if (EquipmentSkill != null)
            {
                Trace.TraceInformation("registered {0} to {1}", EquipmentSkill.GetType().Name, p.Id);
                p.AcquireEquipmentSkill(EquipmentSkill);
            }
            RegisterEquipmentTriggers(p);
        }

        public virtual void UnregisterTriggers(Player p)
        {
            if (EquipmentSkill != null)
            {
                Trace.TraceInformation("unregistered {0} from {1}", EquipmentSkill.GetType().Name, p.Id);
                p.LoseEquipmentSkill(EquipmentSkill);
            }
            UnregisterEquipmentTriggers(p);
        }

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
                    Equipment e = (Equipment)c.Type;
                    Trace.Assert(e != null);
                    List<Card> cardsLost = new List<Card>();
                    List<Card> cardsToDiscard = new List<Card>();
                    cardsToDiscard.Add(c);
                    cardsLost.Add(c);
                    cardsLost.Add(card);
                    Game.CurrentGame.EnterAtomicContext();
                    Game.CurrentGame.PlayerLostCard(p, cardsLost);
                    Game.CurrentGame.HandleCardDiscard(p, cardsToDiscard);
                    Game.CurrentGame.MoveCards(attachMove, null);
                    Game.CurrentGame.ExitAtomicContext();
                    return;
                }
            }
           
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

        public virtual ISkill EquipmentSkill
        {
            get
            {
                return null;
            }
        }
    }
}

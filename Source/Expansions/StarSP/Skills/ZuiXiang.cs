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
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.StarSP.Skills
{
    /// <summary>
    /// 醉乡-限定技，回合开始阶段开始时，你可以展示牌库顶的3张牌置于你的武将牌上，你不可以使用或打出与该些牌同类的牌，所有同类牌对你无效。之后每个你的回合开始阶段，你须重复展示一次，直至该些牌中任意两张点数相同时，将你武将牌上的全部牌置于你的手上。
    /// </summary>
    public class ZuiXiang : TriggerSkill
    {
        PrivateDeckType zxDeck = new PrivateDeckType("ZuiXiang", true);
        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (Owner[ZuiXiangDone] == 1) return;
            if (Owner[ZuiXiangUsed] == 0)
            {
                if (AskForSkillUse())
                {
                    NotifySkillUse(new List<Player>());
                    Owner[ZuiXiangUsed] = 1;
                }
            }
            if (Owner[ZuiXiangUsed] == 1)
            {
                CardsMovement move = new CardsMovement();
                for (int i = 0; i < 3; i++)
                {
                    Game.CurrentGame.SyncImmutableCardAll(Game.CurrentGame.PeekCard(0));
                    Card c = Game.CurrentGame.DrawCard();
                    move.Cards.Add(c);
                }
                move.To = new DeckPlace(Owner, zxDeck);
                Game.CurrentGame.MoveCards(move);
                Dictionary<int, bool> gg = new Dictionary<int, bool>();
                foreach (var card in Game.CurrentGame.Decks[Owner, zxDeck])
                {
                    if (gg.ContainsKey(card.Rank))
                    {
                        Owner[ZuiXiangDone] = 1;
                        break;
                    }
                    gg.Add(card.Rank, true);
                }
                if (Owner[ZuiXiangDone] == 1)
                {
                    move = new CardsMovement();
                    move.Cards = new List<Card>(Game.CurrentGame.Decks[Owner, zxDeck]);
                    move.To = new DeckPlace(Owner, DeckType.Hand);
                    Game.CurrentGame.MoveCards(move);
                }
            }
        }

        void CardUseStopper(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            bool hasBasic = false;
            bool hasTool = false;
            foreach (var card in Game.CurrentGame.Decks[Owner, zxDeck])
            {
                if (CardCategoryManager.IsCardCategory(card.Type.Category, CardCategory.Basic))
                {
                    hasBasic = true;
                }
                if (CardCategoryManager.IsCardCategory(card.Type.Category, CardCategory.Tool))
                {
                    hasTool = true;
                }
            }
            if (CardCategoryManager.IsCardCategory(eventArgs.Card.Type.Category, CardCategory.Basic) && hasBasic)
            {
                throw new TriggerResultException(TriggerResult.Fail);
            }
            if (CardCategoryManager.IsCardCategory(eventArgs.Card.Type.Category, CardCategory.Tool) && hasTool)
            {
                throw new TriggerResultException(TriggerResult.Fail);
            }
        }

        void CardEffectStopper(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            bool hasBasic = false;
            bool hasTool = false;
            foreach (var card in Game.CurrentGame.Decks[Owner, zxDeck])
            {
                if (CardCategoryManager.IsCardCategory(card.Type.Category, CardCategory.Basic))
                {
                    hasBasic = true;
                }
                if (CardCategoryManager.IsCardCategory(card.Type.Category, CardCategory.Tool))
                {
                    hasTool = true;
                }
            }
            if (CardCategoryManager.IsCardCategory(eventArgs.ReadonlyCard.Type.Category, CardCategory.Basic) && hasBasic)
            {
                throw new TriggerResultException(TriggerResult.End);
            }
            if (CardCategoryManager.IsCardCategory(eventArgs.ReadonlyCard.Type.Category, CardCategory.Tool) && hasTool)
            {
                throw new TriggerResultException(TriggerResult.End);
            }
        }

        public static PlayerAttribute ZuiXiangUsed = PlayerAttribute.Register("ZuiXiangUsed");
        public static PlayerAttribute ZuiXiangDone = PlayerAttribute.Register("ZuiXiangDone");

        public ZuiXiang()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                CardUseStopper,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            var trigger3 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard != null; },
                CardEffectStopper,
                TriggerCondition.OwnerIsTarget
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Start], trigger);
            Triggers.Add(GameEvent.PlayerCanUseCard, trigger2);
            Triggers.Add(GameEvent.CardUsageTargetValidating, trigger3);
            IsSingleUse = true;
            DeckCleanup.Add(zxDeck);
        }
    }
}
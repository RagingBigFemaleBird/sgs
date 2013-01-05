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
using Sanguosha.Expansions.Basic.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    public class FuHun : TriggerSkill
    {
        /// <summary>
        /// 父魂-摸牌阶段开始时，你可以放弃摸牌，改为从牌堆顶亮出两张牌并获得之，若亮出的牌颜色不同，你获得技能“武圣”、“咆哮”，直到回合结束。
        /// </summary>
        public class RemoveShengPao : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner) return;
                Owner.LoseAdditionalSkill(FHWuSheng);
                Owner.LoseAdditionalSkill(FHPaoXiao);
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhasePostEnd, this);
            }
            public RemoveShengPao(Player p)
            {
                Owner = p;
            }
        }

        void Run(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            DeckType FuHunDeck = new DeckType("FuHun");
            CardsMovement move = new CardsMovement();
            move.Cards = new List<Card>();
            int toDraw = 2;
            for (int i = 0; i < toDraw; i++)
            {
                Game.CurrentGame.SyncImmutableCardAll(Game.CurrentGame.PeekCard(0));
                Card c = Game.CurrentGame.DrawCard();
                move.Cards.Add(c);
            }
            move.To = new DeckPlace(null, FuHunDeck);
            var result = from c in move.Cards select c.SuitColor;
            Game.CurrentGame.MoveCards(move);
            Game.CurrentGame.HandleCardTransferToHand(null, owner, Game.CurrentGame.Decks[null, FuHunDeck]);
            if (result.Distinct().Count() == toDraw)
            {
                Trigger tri = new RemoveShengPao(owner);
                owner.AcquireAdditionalSkill(FHWuSheng);
                owner.AcquireAdditionalSkill(FHPaoXiao);
                Game.CurrentGame.RegisterTrigger(GameEvent.PhasePostEnd, tri);
            }
            Game.CurrentGame.CurrentPhaseEventIndex++;
            throw new TriggerResultException(TriggerResult.End);
        }

        public FuHun()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Draw], trigger);
            IsAutoInvoked = null;
        }

        private static ISkill FHWuSheng = new WuSheng();
        private static ISkill FHPaoXiao = new PaoXiao();
    }
}

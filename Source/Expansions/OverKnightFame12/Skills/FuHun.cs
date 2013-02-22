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
    /// <summary>
    /// 父魂-摸牌阶段开始时，你可以放弃摸牌，改为从牌堆顶亮出两张牌并获得之，若亮出的牌颜色不同，你获得技能“武圣”、“咆哮”，直到回合结束。
    /// </summary>
    public class FuHun : TriggerSkill
    {
        protected override int GenerateSpecialEffectHintIndex(Player source, List<Player> targets)
        {
            return FuHunEffect;
        }

        public class RemoveShengPao : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner) return;
                Game.CurrentGame.PlayerLoseAdditionalSkill(Owner, fhWuSheng);
                Game.CurrentGame.PlayerLoseAdditionalSkill(Owner, fhPaoXiao);
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhasePostEnd, this);
            }
            public RemoveShengPao(Player p, ISkill s1, ISkill s2)
            {
                Owner = p;
                fhWuSheng = s1;
                fhPaoXiao = s2;
            }
            ISkill fhWuSheng;
            ISkill fhPaoXiao;
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
                c.Log.SkillAction = this;
                c.Log.Source = owner;
                move.Cards.Add(c);
                Game.CurrentGame.NotificationProxy.NotifyShowCard(null, c);
            }
            move.To = new DeckPlace(null, FuHunDeck);
            var result = from c in move.Cards select c.SuitColor;
            bool success = result.Distinct().Count() == toDraw;
            if (success) FuHunEffect = 0;
            else FuHunEffect = 1;
            NotifySkillUse();
            Game.CurrentGame.MoveCards(move, false, Core.Utils.GameDelayTypes.Draw);
            Game.CurrentGame.HandleCardTransferToHand(null, owner, Game.CurrentGame.Decks[null, FuHunDeck]);
            if (success)
            {
                Trigger tri = new RemoveShengPao(owner, fhWuSheng, fhPaoXiao);
                Game.CurrentGame.PlayerAcquireAdditionalSkill(owner, fhWuSheng, HeroTag);
                Game.CurrentGame.PlayerAcquireAdditionalSkill(owner, fhPaoXiao, HeroTag);
                Game.CurrentGame.RegisterTrigger(GameEvent.PhasePostEnd, tri);
            }
            Game.CurrentGame.CurrentPhaseEventIndex++;
            throw new TriggerResultException(TriggerResult.End);
        }

        public FuHun()
        {
            fhWuSheng = new WuSheng();
            fhPaoXiao = new PaoXiao();
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Draw], trigger);
            IsAutoInvoked = null;
        }

        int FuHunEffect;
        ISkill fhWuSheng;
        ISkill fhPaoXiao;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;
using System.Diagnostics;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Wind.Skills
{
    /// <summary>
    /// 不屈-锁定技，当你处于濒死状态时，将牌堆顶的一张牌放置于你的武将牌上，若此牌的点数与武将牌上的其他牌都不同,则你回复至1体力。若出现相同点数则将此牌置入弃牌堆。只要你的武将牌上有牌，你的手牌上限便与武将牌上的牌数量相等。
    /// </summary>
    public class BuQu2 : TriggerSkill
    {
        protected void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            Game.CurrentGame.SyncImmutableCard(Owner, Game.CurrentGame.PeekCard(0));
            Card c1 = Game.CurrentGame.DrawCard();
            bool toDiscard = Game.CurrentGame.Decks[Owner, bq].Any(cd => cd.Rank == c1.Rank);
            CardsMovement move = new CardsMovement();
            move.Cards = new List<Card>() { c1 };
            move.To = new DeckPlace(Owner, bq);
            Game.CurrentGame.MoveCards(move);
            if (toDiscard)
            {
                Game.CurrentGame.PlaceIntoDiscard(Owner, new List<Card>() { c1 });
            }
            else
            {
                Game.CurrentGame.RecoverHealth(Owner, Owner, 1);
            }
        }

        protected void Run2(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            var args = eventArgs as AdjustmentEventArgs;
            args.AdjustmentAmount += Game.CurrentGame.Decks[Owner, bq].Count;
        }

        public BuQu2()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return true; },
                Run,
                TriggerCondition.OwnerIsTarget
            );
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                Run2,
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.PlayerHandCardCapacityAdjustment, trigger2);
            Triggers.Add(GameEvent.PlayerIsAboutToDie, trigger);
            IsEnforced = true;
        }
        public static PrivateDeckType bq = new PrivateDeckType("BuQu", true);
    }
}

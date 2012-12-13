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
    /// 不屈-每当你扣减1点体力时，若你当前体力为0：你可以从牌堆顶亮出一张牌置于你的武将牌上，若该牌的点数与你武将牌上已有的任何一张牌都不同，你不会死亡；若出现相同点数的牌，你进入濒死状态。
    /// </summary>
    public class BuQu : TriggerSkill
    {
        protected void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            DeckType bq = new DeckType("BuQu");
            if (-Owner.Health > Game.CurrentGame.Decks[Owner, bq].Count)
            {
                while (-Owner.Health > Game.CurrentGame.Decks[Owner, bq].Count)
                {
                    Game.CurrentGame.SyncImmutableCard(Owner, Game.CurrentGame.PeekCard(0));
                    Card c1 = Game.CurrentGame.DrawCard();
                    CardsMovement move = new CardsMovement();
                    move.cards = new List<Card>() { c1 };
                    move.to = new DeckPlace(Owner, bq);
                    Game.CurrentGame.MoveCards(move, null);
                }
            }
            else if (Math.Max(0, -Owner.Health) < Game.CurrentGame.Decks[Owner, bq].Count)
            {
                while (-Owner.Health < Game.CurrentGame.Decks[Owner, bq].Count)
                {
                    Card c1 = Game.CurrentGame.Decks[Owner, bq][Game.CurrentGame.Decks[Owner, bq].Count - 1];
                    CardsMovement move = new CardsMovement();
                    move.cards = new List<Card>() { c1 };
                    move.to = new DeckPlace(Owner, bq);
                    Game.CurrentGame.MoveCards(move, null);
                }
            }
            if (Owner.Health > 0) return;
            if (Owner.Health <= 0)
            {
                Dictionary<int, bool> death = new Dictionary<int, bool>();
                foreach (Card c in Game.CurrentGame.Decks[Owner, bq])
                {
                    if (death.ContainsKey(c.Rank))
                    {
                        return;
                    }
                    death.Add(c.Rank, true);
                }
                throw new TriggerResultException(TriggerResult.End);
            }
        }

        public BuQu()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return p.Health <= 0; },
                Run,
                TriggerCondition.OwnerIsTarget
            ) { Type = TriggerType.Skill };
            Triggers.Add(GameEvent.AfterHealthChanged, trigger);
            IsAutoInvoked = null;
        }

    }
}

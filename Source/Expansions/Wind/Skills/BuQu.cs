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
            DeckType bq = new PrivateDeckType("BuQu", true);
            if (1 - Owner.Health > Game.CurrentGame.Decks[Owner, bq].Count)
            {
                int toDraw = 1 - Owner.Health - Game.CurrentGame.Decks[Owner, bq].Count;
                while (toDraw-- > 0)
                {
                    Game.CurrentGame.SyncImmutableCardAll(Game.CurrentGame.PeekCard(0));
                    Card c1 = Game.CurrentGame.DrawCard();
                    CardsMovement move = new CardsMovement();
                    move.Cards = new List<Card>() { c1 };
                    move.To = new DeckPlace(Owner, bq);
                    Game.CurrentGame.MoveCards(move);
                }
            }
            else if (1 + Math.Max(0, -Owner.Health) < Game.CurrentGame.Decks[Owner, bq].Count)
            {
                int toDraw = Game.CurrentGame.Decks[Owner, bq].Count - Math.Max(0, -Owner.Health) - 1;
                while (toDraw-- > 0)
                {
                    Card c1 = Game.CurrentGame.Decks[Owner, bq][Game.CurrentGame.Decks[Owner, bq].Count - 1];
                    CardsMovement move = new CardsMovement();
                    move.Cards = new List<Card>() { c1 };
                    move.To = new DeckPlace(null, DeckType.Discard);
                    Game.CurrentGame.MoveCards(move);
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
                if (Game.CurrentGame.IsDying.Contains(Owner))
                {
                    Stack<Player> backup = new Stack<Player>();
                    while (true)
                    {
                        var t = Game.CurrentGame.IsDying.Pop();
                        if (t == Owner) break;
                        backup.Push(t);
                    }
                    while (backup.Count > 0)
                    {
                        Game.CurrentGame.IsDying.Push(backup.Pop());
                    }
                }
                throw new TriggerResultException(TriggerResult.End);
            }
        }

        public BuQu()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => 
                {
                    DeckType bq = new PrivateDeckType("BuQu", true);
                    if (p.Health > 0 && Game.CurrentGame.Decks[Owner, bq].Count > 0)
                    {
                        int toDraw = Game.CurrentGame.Decks[Owner, bq].Count;
                        while (toDraw-- > 0)
                        {
                            Card c1 = Game.CurrentGame.Decks[Owner, bq][Game.CurrentGame.Decks[Owner, bq].Count - 1];
                            CardsMovement move = new CardsMovement();
                            move.Cards = new List<Card>() { c1 };
                            move.To = new DeckPlace(null, DeckType.Discard);
                            Game.CurrentGame.MoveCards(move);
                        }
                    }
                    return p.Health <= 0; 
                },
                Run,
                TriggerCondition.OwnerIsTarget
            ) { Type = TriggerType.Skill };
            Triggers.Add(GameEvent.AfterHealthChanged, trigger);
            IsAutoInvoked = true;
        }

    }
}

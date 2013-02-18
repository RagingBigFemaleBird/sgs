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
            HealthChangedEventArgs arg = eventArgs as HealthChangedEventArgs;
            if (arg.Delta > 0)
            {
                if (Game.CurrentGame.Decks[Owner, bq].Count <= arg.Delta)
                {
                    Game.CurrentGame.HandleCardDiscard(Owner, Game.CurrentGame.Decks[Owner, bq]);
                    return;
                }
                else
                {
                    List<List<Card>> answer;
                    List<DeckPlace> sourceDecks = new List<DeckPlace>() { new DeckPlace(Owner, bq) };
                    if (!Owner.AskForCardChoice(new CardChoicePrompt("BuQu", Owner),
                        sourceDecks,
                        new List<string>() { "QiPaiDui" },
                        new List<int>() { 1 },
                        new RequireCardsChoiceVerifier(arg.Delta),
                        out answer,
                        null,
                        CardChoiceCallback.GenericCardChoiceCallback))
                    {
                        answer = new List<List<Card>>();
                        answer.Add(Game.CurrentGame.PickDefaultCardsFrom(sourceDecks, arg.Delta));
                    }
                    Game.CurrentGame.HandleCardDiscard(Owner, answer[0]);
                }
                if (!_useBuQu) return;
                List<int> check = new List<int>();
                foreach (var c in Game.CurrentGame.Decks[Owner, bq])
                {
                    if (!check.Contains(c.Rank)) check.Add(c.Rank);
                }
                if (check.Count == Game.CurrentGame.Decks[Owner, bq].Count && Game.CurrentGame.DyingPlayers.Contains(Owner))
                {
                    Owner[Player.SkipDeathComputation] = 1;
                }
                return;
            }
            if (!AskForSkillUse()) { Owner[Player.SkipDeathComputation] = 0; _useBuQu = false; return; }
            _useBuQu = true;
            if (1 - Owner.Health > Game.CurrentGame.Decks[Owner, bq].Count)
            {
                int toDraw = 1 - Owner.Health - Game.CurrentGame.Decks[Owner, bq].Count;
                CardsMovement move = new CardsMovement();
                move.To = new DeckPlace(Owner, bq);
                move.Helper.PrivateDeckHeroTag = HeroTag;
                while (toDraw-- > 0)
                {
                    Game.CurrentGame.SyncImmutableCardAll(Game.CurrentGame.PeekCard(0));
                    Card c1 = Game.CurrentGame.DrawCard();
                    move.Cards.Add(c1);
                }
                Game.CurrentGame.MoveCards(move);
            }
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
                Owner[Player.SkipDeathComputation] = 1;
                NotifySkillUse();
            }
        }

        void LoseBuQu(Player player)
        {
            Game.CurrentGame.HandleCardDiscard(player, Game.CurrentGame.Decks[player, bq]);
            var nArgs = new HealthChangedEventArgs() { Source = null, Delta = 0 };
            nArgs.Targets.Add(player);
            Game.CurrentGame.Emit(GameEvent.AfterHealthChanged, nArgs);
        }

        public override Player Owner
        {
            get
            {
                return base.Owner;
            }
            set
            {
                Player original = base.Owner;
                base.Owner = value;
                if (base.Owner == null && original != null)
                {
                    LoseBuQu(original);
                }
            }
        }

        public BuQu()
        {
            _useBuQu = false;
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    HealthChangedEventArgs arg = a as HealthChangedEventArgs;
                    return p.Health <= 0 || arg.Delta > 0 && Game.CurrentGame.Decks[p, bq].Count > 0;
                },
                Run,
                TriggerCondition.OwnerIsTarget
            ) { AskForConfirmation = false, IsAutoNotify = false, Type = TriggerType.Skill };
            Triggers.Add(GameEvent.AfterHealthChanged, trigger);
            IsAutoInvoked = true;
        }
        private bool _useBuQu;
        private static PrivateDeckType bq = new PrivateDeckType("BuQu", true);
    }
}

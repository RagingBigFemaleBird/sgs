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
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    /// <summary>
    /// 醇醪-回合结束阶段开始时，若你的武将牌上没有牌，你可以将任意数量的【杀】置于你的武将牌上，称为“醇”；当一名角色处于濒死状态时，你可以将一张“醇”置入弃牌堆，视为该角色使用一张【酒】。
    /// </summary>
    public class ChunLao : SaveLifeSkill
    {
        protected override int GenerateSpecialEffectHintIndex(Player source, List<Player> targets, List<Card> cards)
        {
            return Game.CurrentGame.Decks[source, ChunDeck].Count == 0 ? 0 : 1;
        }

        public ChunLao()
        {
            MaxCards = 1;
            MinCards = 1;
            MaxPlayers = 0;
            Helper.OtherDecksUsed.Add(ChunDeck);
            Discarding = true;
            LinkedPassiveSkill = new ChunLaoPassiveSkill();
            (LinkedPassiveSkill as ChunLaoPassiveSkill).ParentSkill = this;
            OwnerOnly = false;
            DeckCleanup.Add(ChunDeck);
        }

        protected override bool VerifyCard(Player source, Card card)
        {
            return card.Place.DeckType == ChunDeck;
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return false;
        }

        protected override bool? SaveLifeVerify(Player source, List<Card> cards, List<Player> players)
        {
            return Game.CurrentGame.Decks[source, ChunDeck].Count > 0;
        }

        public override bool Commit(GameEventArgs arg)
        {
            Game.CurrentGame.HandleCardDiscard(Owner, arg.Cards);
            Player target = Game.CurrentGame.DyingPlayers.Last();
            GameEventArgs args = new GameEventArgs();
            args.Source = target;
            args.Targets.Add(target);
            args.Skill = new CardWrapper(Owner, new Jiu());
            Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
            return true;
        }

        public class ChunLaoPassiveSkill : TriggerSkill
        {
            class ChunLaoStoreChunVerifier : CardsAndTargetsVerifier
            {
                public ChunLaoStoreChunVerifier()
                {
                    MinCards = 1;
                    MaxCards = int.MaxValue;
                    MinPlayers = 0;
                    MaxPlayers = 0;
                    Discarding = false;
                }
                protected override bool VerifyCard(Player source, Card card)
                {
                    return card.Place.DeckType == DeckType.Hand && card.Type is Sha;
                }
            }

            public void StoreChun(Player owner, GameEvent gameEvent, GameEventArgs eventArgs, List<Card> cards, List<Player> players)
            {
                (ParentSkill as ChunLao).NotifyAction(owner, new List<Player>(), cards);
                Game.CurrentGame.HandleCardTransfer(owner, owner, ChunDeck, cards, HeroTag);
            }

            public ISkill ParentSkill { get; set; }
            public ChunLaoPassiveSkill()
            {
                ParentSkill = null;
                var trigger1 = new AutoNotifyUsagePassiveSkillTrigger(
                        this,
                        (p, e, a) => { return Game.CurrentGame.Decks[p, ChunDeck].Count == 0; },
                        StoreChun,
                        TriggerCondition.OwnerIsSource,
                        new ChunLaoStoreChunVerifier()
                    ) { AskForConfirmation = false, IsAutoNotify = false };
                Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger1);
            }
        }
        public static PrivateDeckType ChunDeck = new PrivateDeckType("Chun", false);
    }
}

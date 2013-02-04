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
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.StarSP.Skills
{
    /// <summary>
    /// 大喝―出牌阶段，你可以与一名其他角色拼点。若你赢，该角色的非红心【闪】无效直到回合结束，你可将该角色拼点的牌交给场上一名体力不多于你的角色。若你没赢，你须展示手牌并选择一张弃置。每阶段限一次。
    /// </summary>
    public class DaHe : AutoVerifiedActiveSkill
    {
        public class DaHeVerifier : CardsAndTargetsVerifier
        {
            public DaHeVerifier()
            {
                MinCards = 0;
                MaxCards = 0;
                MinPlayers = 1;
                MaxPlayers = 1;
            }
            protected override bool VerifyPlayer(Player source, Player player)
            {
                return source.Health >= player.Health;
            }
        }

        public static PlayerAttribute DaHeUsed = PlayerAttribute.Register("DaHeUsed", true);

        public override bool Commit(GameEventArgs arg)
        {
            Owner[DaHeUsed] = 1;
            Card card1, card2;
            Game.CurrentGame.PinDianReturnCards(Owner, arg.Targets[0], out card1, out card2, this);
            bool win = false;
            if (card1.Rank > card2.Rank)
            {
                win = true;
                (LinkedPassiveSkill as DaHePassive).DaHePlayer = arg.Targets[0];
                ISkill skill;
                List<Card> cards;
                List<Player> players;
                if (Owner.AskForCardUsage(new CardUsagePrompt("DaHe", arg.Targets[0]), new DaHeVerifier(), out skill, out cards, out players))
                {
                    Game.CurrentGame.EnterAtomicContext();
                    Game.CurrentGame.HandleCardTransferToHand(arg.Targets[0], players[0], new List<Card>() { card2 });
                    Game.CurrentGame.PlaceIntoDiscard(Owner, new List<Card>() { card1 });
                    Game.CurrentGame.ExitAtomicContext();
                    return true;
                }
            }
            Game.CurrentGame.EnterAtomicContext();
            Game.CurrentGame.PlaceIntoDiscard(Owner, new List<Card>() { card1 });
            Game.CurrentGame.PlaceIntoDiscard(arg.Targets[0], new List<Card>() { card2 });
            Game.CurrentGame.ExitAtomicContext();
            if (!win && Owner.HandCards().Count > 0)
            {
                Game.CurrentGame.ForcePlayerDiscard(Owner,(p, i) => { return 1 - i; }, false);
                Game.CurrentGame.SyncImmutableCardsAll(Owner.HandCards());
                Game.CurrentGame.ShowHandCards(Owner, Owner.HandCards());
            }
            return true;
        }

        public DaHe()
        {
            MinCards = 0;
            MaxCards = 0;
            MinPlayers = 1;
            MaxPlayers = 1;
            LinkedPassiveSkill = new DaHePassive();
            (LinkedPassiveSkill as DaHePassive).DaHePlayer = null;
        }

        protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
        {
            return source[DaHeUsed] == 0;
        }

        protected override bool VerifyCard(Player source, Card card)
        {
            return true;
        }

        protected override bool VerifyPlayer(Player source, Player player)
        {
            return source != player;
        }

        public class DaHePassive : TriggerSkill
        {
            public Player DaHePlayer { get; set; }
            public DaHePassive()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) =>
                    {
                        return DaHePlayer != null && a.Source == DaHePlayer && a.ReadonlyCard.Type is Shan && a.ReadonlyCard.Suit != SuitType.Heart;
                    },
                    (p, e, a) => { throw new TriggerResultException(TriggerResult.End); },
                    TriggerCondition.Global
                );
                var trigger2 = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { DaHePlayer = null; },
                    TriggerCondition.Global
                ) { IsAutoNotify = false };
                Triggers.Add(GameEvent.CardUsageTargetValidating, trigger);
                Triggers.Add(GameEvent.PhasePostEnd, trigger2);
                IsEnforced = true;
            }
        }


    }

}

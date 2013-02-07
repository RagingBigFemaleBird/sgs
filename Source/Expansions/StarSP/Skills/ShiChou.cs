using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.StarSP.Skills
{
    /// <summary>
    /// 誓仇–主公技，限定技，回合开始时，你可以交给一名蜀势力的其他角色两张牌，若如此做，直到该角色进入濒死状态前，当你受到伤害时，将该伤害转移给该角色，然后该角色摸等同于转移的伤害数值的牌。
    /// </summary>
    public class ShiChou : TriggerSkill
    {
        class ShiChouVerifier : CardsAndTargetsVerifier
        {
            public ShiChouVerifier()
            {
                MaxCards = 2;
                MinCards = 2;
                MaxPlayers = 1;
                MinPlayers = 1;
                Discarding = false;
            }
            protected override bool VerifyPlayer(Player source, Player player)
            {
                return source != player && player.Allegiance == Allegiance.Shu && source[ShiChouSource[player]] == 0;
            }
        }

        class ShiChouProtect : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (!eventArgs.Targets.Contains(Owner)) return;
                DamageEventArgs damageArgs = eventArgs as DamageEventArgs;
                ReadOnlyCard rCard = new ReadOnlyCard(damageArgs.ReadonlyCard);
                rCard[ShiChouDamage] = 1;
                target[ShiChouTarget[Owner]] ++;
                Game.CurrentGame.DoDamage(damageArgs.Source, target, Owner, damageArgs.Magnitude, damageArgs.Element, damageArgs.Card, rCard);
                throw new TriggerResultException(TriggerResult.End);
            }

            Player target;
            public ShiChouProtect(Player source, Player target)
            {
                Owner = source;
                this.target = target;
            }
        }

        class ShiChouDrawCards : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (Owner[ShiChouTarget[source]] == 0 || !eventArgs.Targets.Contains(Owner) || eventArgs.ReadonlyCard[ShiChouDamage] == 0)
                {
                    return;
                }
                Owner[ShiChouTarget[source]]--;
                Game.CurrentGame.DrawCards(Owner, (eventArgs as DamageEventArgs).Magnitude);
            }

            Player source;
            public ShiChouDrawCards(Player target, Player ShiChouSource)
            {
                Owner = target;
                source = ShiChouSource;
            }
        }

        class ShiChouRemoval : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (!eventArgs.Targets.Contains(Owner)) return ;
                Owner[ShiChouSource[source]] = 0;
                Owner[ShiChouStatus] = 0;
                Game.CurrentGame.UnregisterTrigger(GameEvent.DamageInflicted, protectTrigger);
                Game.CurrentGame.UnregisterTrigger(GameEvent.DamageComputingFinished, drawCardsTrigger);
                Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerIsAboutToDie, this);
            }

            Player source;
            Trigger protectTrigger;
            Trigger drawCardsTrigger;
            public ShiChouRemoval(Player target, Player source, Trigger protect, Trigger drawCards)
            {
                Owner = target;
                this.source = source;
                protectTrigger = protect;
                drawCardsTrigger = drawCards;
                Priority = int.MaxValue;
            }
        }

        bool CanTriggerShiChou(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            return Owner[ShiChouUsed] == 0 && Game.CurrentGame.AlivePlayers.Any(p => { return p != Owner && p.Allegiance == Allegiance.Shu; });
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (Owner.AskForCardUsage(new CardUsagePrompt("ShiChou", this), new ShiChouVerifier(), out skill, out cards, out players))
            {
                NotifySkillUse(players);
                Owner[ShiChouUsed] = 1;
                players[0][ShiChouSource[Owner]] = 1;
                Game.CurrentGame.HandleCardTransferToHand(Owner, players[0], cards);
                players[0][ShiChouStatus] = 1;
                Trigger tri1 = new ShiChouProtect(Owner, players[0]);
                Trigger tri2 = new ShiChouDrawCards(players[0], Owner);
                Trigger tri3 = new ShiChouRemoval(players[0], Owner, tri1, tri2);
                Game.CurrentGame.RegisterTrigger(GameEvent.DamageInflicted, tri1);
                Game.CurrentGame.RegisterTrigger(GameEvent.DamageComputingFinished, tri2);
                Game.CurrentGame.RegisterTrigger(GameEvent.PlayerIsAboutToDie, tri3);
            }
        }

        public ShiChou()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                      this,
                      CanTriggerShiChou,
                      Run,
                      TriggerCondition.OwnerIsSource
                  ) { AskForConfirmation = false, IsAutoNotify = false};
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Start], trigger);

            IsRulerOnly = true;
            IsSingleUse = true;
        }

        private static CardAttribute ShiChouDamage = CardAttribute.Register("ShiChouDamage");
        private static PlayerAttribute ShiChouUsed = PlayerAttribute.Register("ShiChouUsed");
        private static PlayerAttribute ShiChouSource = PlayerAttribute.Register("ShiChouSource");
        private static PlayerAttribute ShiChouTarget = PlayerAttribute.Register("ShiChouTarget");
        private static PlayerAttribute ShiChouStatus = PlayerAttribute.Register("ShiChou", false, false, true);
    }
}

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

namespace Sanguosha.Expansions.Fire.Skills
{
    /// <summary>
    /// 大雾-回合结束阶段开始时，你可以弃掉X张“星”，指定X名角色，直到你的下回合开始，防止他们受到的除雷电伤害外的所有伤害。
    /// </summary>
    public class DaWu : TriggerSkill
    {
        class DaWuVerifier : CardsAndTargetsVerifier
        {
            public DaWuVerifier(int qxCount)
            {
                MaxPlayers = qxCount;
                MinPlayers = 1;
                MaxCards = qxCount;
                MinCards = 1;
                Helper.OtherDecksUsed.Add(QiXing.QiXingDeck);
            }

            protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
            {
                int cp, cc;
                if (players == null) cp = 0; else cp = players.Count;
                if (cards == null) cc = 0; else cc = cards.Count;
                if (cp != cc) return null;
                return true;
            }
            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Place.DeckType == QiXing.QiXingDeck;
            }

        }

        List<Player> dawuTargets;
        public static readonly PlayerAttribute DaWuMark = PlayerAttribute.Register("DaWu", false, true);

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            int qxCount = Game.CurrentGame.Decks[Owner, QiXing.QiXingDeck].Count;
            if (Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("DaWu"), new DaWuVerifier(qxCount), out skill, out cards, out players))
            {
                NotifySkillUse(players);
                foreach (var mark in players)
                {
                    mark[DaWuMark] = 1;
                }
                dawuTargets = players;
                Game.CurrentGame.HandleCardDiscard(null, cards);
                Trigger tri = new DaWuProtect();
                Game.CurrentGame.RegisterTrigger(GameEvent.DamageComputingStarted, tri);
                Game.CurrentGame.RegisterTrigger(GameEvent.PhaseBeginEvents[TurnPhase.Start], new DawuRemoval(Owner, tri, dawuTargets));
            }
        }

        class DawuRemoval : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                while (!qixingOwner.IsDead)
                {
                    if (eventArgs.Source != qixingOwner)
                    {
                        return;
                    }
                    foreach (var mark in dawuTargets) { mark[DaWuMark] = 0; }
                    dawuTargets.Clear();
                    break;
                }
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhaseBeginEvents[TurnPhase.Start], this);
                Game.CurrentGame.UnregisterTrigger(GameEvent.DamageComputingStarted, dawuProtect);
            }
            Player qixingOwner;
            Trigger dawuProtect;
            List<Player> dawuTargets;
            public DawuRemoval(Player p, Trigger trigger, List<Player> dawuTargets)
            {
                qixingOwner = p;
                dawuProtect = trigger;
                this.dawuTargets = dawuTargets;
            }
        }

        class DaWuProtect : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                var args = eventArgs as DamageEventArgs;
                if (args.Element == DamageElement.Lightning || eventArgs.Targets[0][DaWuMark] == 0)
                {
                    return;
                }
                throw new TriggerResultException(TriggerResult.End);
            }
        }

        class DaWuOnDeath : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Targets[0] != Owner) return;
                foreach (Player target in dawuTargets)
                {
                    target[DaWuMark] = 0;
                }
                Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerIsDead, this);
            }
            List<Player> dawuTargets;
            public DaWuOnDeath(Player p, List<Player> dawuTargets)
            {
                Owner = p;
                this.dawuTargets = dawuTargets;
            }
        }

        public DaWu()
        {
            dawuTargets = new List<Player>();
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Game.CurrentGame.Decks[Owner, QiXing.QiXingDeck].Count > 0; },
                Run,
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger);

            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { Game.CurrentGame.RegisterTrigger(GameEvent.PlayerIsDead, new DaWuOnDeath(p, dawuTargets)); },
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PlayerGameStartAction, trigger2);

            IsAutoInvoked = null;
        }
    }
}

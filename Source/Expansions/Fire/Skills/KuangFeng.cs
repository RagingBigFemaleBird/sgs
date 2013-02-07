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
    /// 狂风-回合结束阶段开始时，你可以弃置1张“星”，指定一名角色，直到你的下回合开始，该角色每次受到的火焰伤害+1。
    /// </summary>
    public class KuangFeng : TriggerSkill
    {
        class KuangFengVerifier : CardsAndTargetsVerifier
        {
            public KuangFengVerifier()
            {
                MaxPlayers = 1;
                MinPlayers = 1;
                MaxCards = 1;
                MinCards = 1;
                Helper.OtherDecksUsed.Add(QiXing.QiXingDeck);
            }

            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Place.DeckType == QiXing.QiXingDeck;
            }

        }

        static Player kuangfengTarget;
        public static readonly PlayerAttribute KuangFengMark = PlayerAttribute.Register("KuangFeng", false, true);

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("KuangFeng"), new KuangFengVerifier(), out skill, out cards, out players))
            {
                NotifySkillUse(players);
                kuangfengTarget = players[0];
                kuangfengTarget[KuangFengMark] = 1;
                Game.CurrentGame.HandleCardDiscard(null, cards);
                Trigger tri = new KuangFengDamage();
                Game.CurrentGame.RegisterTrigger(GameEvent.DamageComputingStarted, tri);
                Game.CurrentGame.RegisterTrigger(GameEvent.PhaseBeginEvents[TurnPhase.Start], new KuangFengRemoval(Owner, tri));
            }
        }

        class KuangFengRemoval : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                while (!qixingOwner.IsDead)
                {
                    if (eventArgs.Source != qixingOwner)
                    {
                        return;
                    }
                    kuangfengTarget[KuangFengMark] = 0;
                    kuangfengTarget = null;
                    break;
                }
                Game.CurrentGame.UnregisterTrigger(GameEvent.PhaseBeginEvents[TurnPhase.Start], this);
                Game.CurrentGame.UnregisterTrigger(GameEvent.DamageComputingStarted, kuangfengDamage);
            }
            Player qixingOwner;
            Trigger kuangfengDamage;
            public KuangFengRemoval(Player p, Trigger trigger)
            {
                qixingOwner = p;
                kuangfengDamage = trigger;
            }
        }

        class KuangFengDamage : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                var args = eventArgs as DamageEventArgs;
                if (args.Element != DamageElement.Fire || eventArgs.Targets[0][KuangFengMark] == 0)
                {
                    return;
                }
                args.Magnitude++;
            }
        }

        class KuangFengOnDeath : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Targets[0] != Owner) return;
                if (kuangfengTarget != null) kuangfengTarget[KuangFengMark] = 0;
                Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerIsDead, this);
            }
            public KuangFengOnDeath(Player p)
            {
                Owner = p;
            }
        }

        public KuangFeng()
        {
            kuangfengTarget = null;
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Game.CurrentGame.Decks[Owner, QiXing.QiXingDeck].Count > 0; },
                Run,
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger);

            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { Game.CurrentGame.RegisterTrigger(GameEvent.PlayerIsDead, new KuangFengOnDeath(p)); },
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PlayerGameStartAction, trigger2);

            IsAutoInvoked = null;
        }

    }
}

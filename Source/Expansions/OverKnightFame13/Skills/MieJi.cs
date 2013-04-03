using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using System.Diagnostics;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.OverKnightFame13.Skills
{
    public class MieJi : TriggerSkill
    {
        class MieJiVerifier : CardsAndTargetsVerifier
        {
            CardHandler handler;
            Player existingTarget;
            ICard existingCard;
            public MieJiVerifier(Player p, ICard c, CardHandler handler)
            {
                existingCard = c;
                existingTarget = p;
                this.handler = handler;
                MaxCards = 0;
                MinCards = 0;
                MaxPlayers = Int16.MaxValue;
                MinPlayers = 1;
            }
            protected override bool? AdditionalVerify(Player source, List<Card> cards, List<Player> players)
            {
                if (cards != null && cards.Count > 0) return false;
                if (players == null) players = new List<Player>();
                var actualTargets = handler.ActualTargets(source, players, existingCard);
                if (actualTargets.Count > 1) return false;
                if (actualTargets.Contains(existingTarget)) return false;
                var ret = handler.VerifyTargets(source, existingCard, players);
                if (ret == VerifierResult.Partial) return null;
                if (ret == VerifierResult.Fail) return false;
                return true;
            }
            protected override bool VerifyPlayer(Player source, Player player)
            {
                return true;
            }
        }

        protected void Run(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (owner.AskForCardUsage(new CardUsagePrompt("MieJi", this), new MieJiVerifier(eventArgs.Targets[0], eventArgs.Card, eventArgs.Card.Type), out skill, out cards, out players))
            {
                NotifySkillUse(players);
                eventArgs.UiTargets.AddRange(players);
                eventArgs.Targets = eventArgs.Card.Type.ActualTargets(eventArgs.Source, eventArgs.UiTargets, eventArgs.Card);
            }
        }

        public MieJi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => 
                {
                    return a.Card.Type.IsCardCategory(CardCategory.ImmediateTool) && a.Card.SuitColor == SuitColorType.Black && a.Targets.Count == 1; 
                },
                Run,
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.CardUsageTargetConfirmed, trigger);
            IsAutoInvoked = true;
        }
    }
}

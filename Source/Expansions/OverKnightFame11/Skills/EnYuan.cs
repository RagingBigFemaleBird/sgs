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

namespace Sanguosha.Expansions.OverKnightFame11.Skills
{
    /// <summary>
    /// 恩怨-你每次获得一名其他角色两张或更多的牌时，可以令其摸一张牌；每当你受到1点伤害后，你可以令伤害来源选择一项：交给你一张手牌，或失去1点体力。
    /// </summary>
    public class EnYuan : TriggerSkill
    {
        protected override int GenerateSpecialEffectHintIndex(Player source, List<Player> targets)
        {
            return EnYuanEffect;
        }

        public class EnYuanVerifier : CardsAndTargetsVerifier
        {
            public EnYuanVerifier()
            {
                MinCards = 1;
                MaxCards = 1;
                MaxPlayers = 0;
            }
            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Place.DeckType == DeckType.Hand;
            }
        }

        public void Yuan(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            int magnitude = (eventArgs as DamageEventArgs).Magnitude;
            while (magnitude-- > 0)
            {
                if (!AskForSkillUse())
                    break;
                EnYuanEffect = 1;
                NotifySkillUse();
                ISkill skill;
                List<Card> cards;
                List<Player> players;
                if (eventArgs.Source.AskForCardUsage(new CardUsagePrompt("EnYuan", owner), new EnYuanVerifier(), out skill, out cards, out players))
                {
                    Game.CurrentGame.HandleCardTransferToHand(eventArgs.Source, owner, cards);
                }
                else
                {
                    Game.CurrentGame.LoseHealth(eventArgs.Source, 1);
                }
            }
        }

        bool CardOriginalOwnerCheck(Card card)
        {
            return card.HistoryPlace1.Player != null && (card.HistoryPlace1.DeckType == DeckType.Hand || card.HistoryPlace1.DeckType == DeckType.Equipment || card.HistoryPlace1.DeckType is StagingDeckType);
        }

        bool EnVerifier(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            enSources.Clear();
            var result = from card in eventArgs.Cards where CardOriginalOwnerCheck(card) && (card.Place.DeckType == DeckType.Hand || card.Place.DeckType == DeckType.Equipment) && card.Place.Player != Owner select card;
            if (result.Count() == 0)
            {
                return false;
            }
            Dictionary<Player, int> dic = new Dictionary<Player, int>();
            foreach (Card card in result)
            {
                if (!dic.Keys.Contains(card.HistoryPlace1.Player)) dic[card.HistoryPlace1.Player] = 0;
                dic[card.HistoryPlace1.Player]++;
            }
            foreach (Player p in dic.Keys)
            {
                if (dic[p] >= 2)
                    enSources.Add(p);
            }
            return enSources.Count > 0;
        }

        public void En(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            foreach (Player source in enSources)
            {
                int answer = 0;
                Owner.AskForMultipleChoice(new MultipleChoicePrompt("EnYuan", source), OptionPrompt.YesNoChoices, out answer);
                if (answer == 1)
                {
                    EnYuanEffect = 0;
                    NotifySkillUse();
                    Game.CurrentGame.DrawCards(source, 1);
                }
            }
        }

        public EnYuan()
        {
            enSources = new List<Player>();
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                EnVerifier,
                En,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.CardsAcquired, trigger);
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Source != null; },
                Yuan,
                TriggerCondition.OwnerIsTarget
            ) { IsAutoNotify = false, AskForConfirmation = false };
            Triggers.Add(GameEvent.AfterDamageInflicted, trigger2);
            IsAutoInvoked = null;
        }
        List<Player> enSources;
        private int EnYuanEffect;
    }
}

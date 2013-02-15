using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Assassin.Skills
{
    /// <summary>
    /// 天命-当你成为【杀】的目标时：你可以弃置两张牌（不足则全弃，无牌则不弃），然后摸两张牌；若此时全场当前的体力值最多的角色仅有一名且不是你，该角色也可以如此做。
    /// </summary>
    public class TianMing : TriggerSkill
    {
        protected override int GenerateSpecialEffectHintIndex(Player source, List<Player> targets)
        {
            return TianMingEffect;
        }
        class TianMingVerifier : CardUsageVerifier
        {
            int discardCount;
            public TianMingVerifier(int count)
            {
                discardCount = count;
            }
            public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if (players != null && players.Count > 1 || skill != null)
                {
                    return VerifierResult.Fail;
                }
                if (cards != null && cards.Count > discardCount)
                {
                    return VerifierResult.Fail;
                }
                if (cards == null && discardCount != 0)
                {
                    return VerifierResult.Partial;
                }
                if (cards != null && cards.Count > 0 && !Game.CurrentGame.PlayerCanDiscardCards(source, cards))
                {
                    return VerifierResult.Fail;
                }
                if (cards.Count < discardCount)
                {
                    return VerifierResult.Partial;
                }
                return VerifierResult.Success;
            }

            public override IList<CardHandler> AcceptableCardTypes
            {
                get { return null; }
            }
        }

        bool TianMingProcess(Player player)
        {
            int discardedCount = Math.Min(player.HandCards().Count + player.Equipments().Count, 2);
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            CardUsagePrompt prompt = new CardUsagePrompt("TianMing");
            CardUsagePrompt otherPrompt = new CardUsagePrompt("TianMingOther", discardedCount);
            CardUsagePrompt otherIsNakedPrompt = new CardUsagePrompt("TianMingOtherIsNaked");
            if (player.AskForCardUsage(player == Owner ? prompt : (discardedCount == 0 ? otherIsNakedPrompt : otherPrompt), new TianMingVerifier(discardedCount), out skill, out cards, out players))
            {
                if (player == Owner)
                {
                    TianMingEffect = 0;
                }
                else if (player.IsMale)
                {
                    TianMingEffect = 1;
                }
                else
                {
                    TianMingEffect = 2;
                }
                NotifySkillUse();
                if (cards.Count > 0)
                {
                    Game.CurrentGame.HandleCardDiscard(player, cards);
                }
                Game.CurrentGame.DrawCards(player, 2);
                return true;
            }
            return false;
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (TianMingProcess(Owner))
            {
                List<Player> temp = Game.CurrentGame.AlivePlayers;
                int maxHealth = temp.Max(pl => pl.Health);
                var maxHealther = from p in temp where p.Health == maxHealth select p;
                if (maxHealther.Count() > 1 || maxHealther.First() == Owner) return;
                TianMingProcess(maxHealther.First());
            }
        }

        int TianMingEffect;
        public TianMing()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard.Type is Sha; },
                Run,
                TriggerCondition.OwnerIsTarget
            ) { IsAutoNotify = false, AskForConfirmation = false };
            Triggers.Add(GameEvent.CardUsageTargetConfirming, trigger);
            IsAutoInvoked = null;
        }
    }
}

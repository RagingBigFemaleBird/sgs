using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 放逐-每当你受到一次伤害后，可令一名其他角色摸X张牌(X为你已损失的体力值)，然后该角色将其武将牌翻面。
    /// </summary>
    public class FangZhu : TriggerSkill
    {
        public class FangZhuVerifier : ICardUsageVerifier
        {
            public UiHelper Helper { get { return new UiHelper(); } }
            public VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if (skill != null || (cards != null && cards.Count != 0))
                {
                    return VerifierResult.Fail;
                }
                if (players == null || players.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                foreach (Player p in players)
                {
                    if (p == source)
                    {
                        return VerifierResult.Fail;
                    }
                }
                if (players.Count > 1)
                {
                    return VerifierResult.Fail;
                }
                return VerifierResult.Success;
            }

            public IList<CardHandler> AcceptableCardTypes
            {
                get { throw new NotImplementedException(); }
            }

            public VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                return FastVerify(source, skill, cards, players);
            }
        }

        protected void OnAfterDamageInflicted(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("FangZhu"), new FangZhuVerifier(), out skill, out cards, out players))
            {
                NotifySkillUse(players);
                players[0].IsImprisoned = !players[0].IsImprisoned;
                Game.CurrentGame.DrawCards(players[0], Owner.MaxHealth - Owner.Health);
            }
        }

        public FangZhu()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                OnAfterDamageInflicted,
                TriggerCondition.OwnerIsTarget
            ) { IsAutoNotify = false, AskForConfirmation = false };
            Triggers.Add(GameEvent.AfterDamageInflicted, trigger);
            IsAutoInvoked = null;
        }
    }
}

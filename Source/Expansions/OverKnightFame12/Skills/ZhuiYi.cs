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

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    /// <summary>
    /// 【追忆】——你死亡时，可以令一名其他角色（杀死你的角色除外）摸三张牌并回复1点体力。
    /// </summary>
    public class ZhuiYi : TriggerSkill
    {
        public class ZhuiYiVerifier : ICardUsageVerifier
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
                    if (p == source || p.Id==source[ZhuiYiKiller])
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

        protected void OnDead(Player owner, GameEvent gameEvent, GameEventArgs eventArgs, List<Card> cards, List<Player> players)
        {
            Game.CurrentGame.DrawCards(players[0],3);
            Game.CurrentGame.RecoverHealth(Owner, players[0], 1);
        }

        public ZhuiYi()
        {
            var trigger = new AutoNotifyUsagePassiveSkillTrigger(
                this,
                (p, e, a) => { p[ZhuiYiKiller] = a.Source.Id; return true; },
                OnDead,
                TriggerCondition.OwnerIsTarget,
                new ZhuiYiVerifier()
            );
            Triggers.Add(GameEvent.PlayerIsDead, trigger);
            IsAutoInvoked = null;
        }
        public static PlayerAttribute ZhuiYiKiller = PlayerAttribute.Register("ZhuiYiKiller");

    }
}

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
    /// 追忆-你死亡时，可以令一名其他角色（杀死你的角色除外）摸三张牌并回复1点体力。
    /// </summary>
    public class ZhuiYi : TriggerSkill
    {
        public class ZhuiYiVerifier : CardsAndTargetsVerifier
        {
            Player killer;
            public ZhuiYiVerifier(Player killer)
            {
                this.killer = killer;
                MaxCards = 0;
                MinCards = 0;
                MaxPlayers = 1;
                MinPlayers = 1;
            }
            protected override bool VerifyPlayer(Player source, Player player)
            {
                return player != killer && player != source;
            }
        }

        protected void OnDead(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (owner.AskForCardUsage(new CardUsagePrompt("ZhuiYi"), new ZhuiYiVerifier(eventArgs.Source), out skill, out cards, out players))
            {
                NotifySkillUse(players);
                Game.CurrentGame.DrawCards(players[0], 3);
                Game.CurrentGame.RecoverHealth(Owner, players[0], 1);
            }
        }

        public ZhuiYi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                OnDead,
                TriggerCondition.OwnerIsTarget
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PlayerIsDead, trigger);
            IsAutoInvoked = null;
        }
    }
}

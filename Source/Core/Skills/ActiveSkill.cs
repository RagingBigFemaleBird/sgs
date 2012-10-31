using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;

namespace Sanguosha.Core.Skills
{
    public abstract class ActiveSkill : ISkill
    {
        public virtual UiHelper Helper { get { return new UiHelper(); } }
        /// <summary>
        /// 检查主动技的合法性。
        /// </summary>
        /// <param name="arg">参数</param>
        /// <param name="card">输出卡牌</param>
        /// <returns></returns>
        public abstract VerifierResult Validate(GameEventArgs arg);

        /// <summary>
        /// 提交主动技的发动请求。
        /// </summary>
        /// <param name="arg">参数</param>
        /// <returns>true if 可以打出, false if 不可打出</returns>
        public abstract bool Commit(GameEventArgs arg);

        public virtual Players.Player Owner { get; set; }

        public virtual void CardRevealPolicy(Players.Player p, List<Card> cards, List<Players.Player> players)
        {
        }

        protected void NotifyAction(Players.Player source, List<Players.Player> targets, List<Card> cards)
        {
            ActionLog log = new ActionLog();
            log.GameAction = GameAction.None;
            log.CardAction = null;
            log.SkillAction = this;
            log.Source = source;
            log.Targets = targets;
            log.Cards = cards;
            Games.Game.CurrentGame.NotificationProxy.NotifySkillUse(log);
        }

        public virtual bool isRulerOnly { get { return false; } }
        public virtual bool isSingleUse { get { return false; } }
        public virtual bool isAwakening { get { return false; } }
        public bool isEnforced { get { return false; } }
    }
}

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
        public UiHelper UiHelper { get; protected set; }

        public ActiveSkill()
        {
            UiHelper = new UiHelper();
        }

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

        public void NotifyAction(Players.Player source, List<Players.Player> targets, List<Card> cards)
        {
            ActionLog log = new ActionLog();
            log.GameAction = GameAction.None;
            log.CardAction = null;
            log.SkillAction = this;
            log.Source = source;
            List<Players.Player> ft, st;
            TargetsSplit(targets, out ft, out st);
            log.Targets = ft;
            log.SecondaryTargets = st;
            foreach (Card c in cards)
            {
                if (c.Log == null)
                {
                    c.Log = new ActionLog();
                }
                c.Log.SkillAction = this;
            }
            Games.Game.CurrentGame.NotificationProxy.NotifySkillUse(log);
        }

        protected void TargetsSplit(List<Players.Player> targets, out List<Players.Player> firstTargets, out List<Players.Player> secondaryTargets)
        {
            firstTargets = new List<Players.Player>(targets);
            secondaryTargets = null;
        }

        public virtual bool IsRulerOnly { get { return false; } }
        public virtual bool IsSingleUse { get { return false; } }
        public virtual bool IsAwakening { get { return false; } }
        public bool IsEnforced { get { return false; } }
    }
}

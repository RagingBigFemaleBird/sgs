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
    }
}

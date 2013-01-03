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
    public class XianZhen : ActiveSkill
    {
        /// <summary>
        /// 陷阵-出牌阶段，你可以与一名其他角色拼点。若你赢，你获得以下技能直到回合结束：你无视与该角色的距离及其防具；你对该角色使用【杀】时无次数限制。若你没赢，你不能使用【杀】，直到回合结束。每阶段限一次。
        /// </summary>
        public override VerifierResult Validate(GameEventArgs arg)
        {
            if (Owner[XianZhenUsed] != 0)
            {
                return VerifierResult.Fail;
            }
            List<Card> cards = arg.Cards;
            if (cards != null && cards.Count > 0)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets == null || arg.Targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if ((arg.Targets != null && arg.Targets.Count > 1) || arg.Targets[0].HandCards().Count == 0)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets[0] == Owner || Owner.HandCards().Count == 0)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public override bool Commit(GameEventArgs arg)
        {
            return true;
        }
        private static PlayerAttribute XianZhenUsed = PlayerAttribute.Register("XianZhenUsed", true);
    }
}

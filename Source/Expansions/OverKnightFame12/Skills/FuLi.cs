using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;
using System.Diagnostics;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    /// <summary>
    /// 伏枥-限定技，当你处于濒死状态时，你可以将体力回复至X点（X为现存势力数），然后将你的武将牌翻面。
    /// </summary>
    public class FuLi : SaveLifeSkill
    {
        public FuLi()
        {
            IsSingleUse = true;
            Helper.HasNoConfirmation = true;
        }

        protected override bool? SaveLifeVerify(Player source, List<Card> cards, List<Player> players)
        {
            return Owner[FuLiUsed] == 0;
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[FuLiUsed] = 1;
            Owner.Health = Game.CurrentGame.NumberOfAliveAllegiances;
            Owner.IsImprisoned = !Owner.IsImprisoned;
            return true;
        }

        public static PlayerAttribute FuLiUsed = PlayerAttribute.Register("FuLiUsed", false);

    }
}

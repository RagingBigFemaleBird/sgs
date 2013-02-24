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

namespace Sanguosha.Expansions.Wind.Skills
{
    /// <summary>
    /// 武神-锁定技，你的红桃手牌均视为【杀】，你使用红桃【杀】时无距离限制。
    /// </summary>
    public class WuShen : EnforcedCardTransformSkill
    {
        class WuShenShaTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                ShaEventArgs args = (ShaEventArgs)eventArgs;
                Trace.Assert(args != null);
                if (args.Source != Owner)
                {
                    return;
                }
                if (args.Card.Suit != SuitType.Heart)
                {
                    return;
                }
                for (int i = 0; i < args.RangeApproval.Count; i++)
                {
                    args.RangeApproval[i] = true;
                }
            }
        }

        protected override bool CardVerifier(ICard card)
        {
            return card.Suit == SuitType.Heart;
        }

        protected override void TransfromAction(Player Owner, ICard card)
        {
            card.Type = new RegularSha();
        }

        public WuShen()
        {
            Decks.Add(DeckType.Hand);
            Triggers.Add(Sha.PlayerShaTargetValidation, new WuShenShaTrigger());
        }
    }
}

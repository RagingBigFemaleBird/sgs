using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;

namespace Sanguosha.Expansions.Fire.Skills
{
    /// <summary>
    /// 连环-出牌阶段，你可以将一张梅花牌当【铁索连环】使用。
    /// </summary>
    public class LianHuan : OneToOneCardTransformSkill
    {
        public LianHuan()
        {
            HandCardOnly = true;
        }

        public override bool VerifyInput(Card card, object arg)
        {
            return card.Suit == SuitType.Club;
        }

        public override void NotifyAction(Core.Players.Player source, List<Core.Players.Player> targets, CompositeCard card)
        {
            if (card.Type.IsReforging(source, this, card.Subcards, targets))
            {
                foreach (var c in card.Subcards)
                {
                    c.Log.SkillAction = this;
                }
                return;
            }
            base.NotifyAction(source, targets, card);
        }

        public override CardHandler PossibleResult
        {
            get { return new TieSuoLianHuan(); }
        }
    }
}

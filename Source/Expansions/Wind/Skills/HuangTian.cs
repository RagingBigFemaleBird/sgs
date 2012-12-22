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
using Sanguosha.Core.Heroes;

namespace Sanguosha.Expansions.Wind.Skills
{
    /// <summary>
    /// 黄天―主公技，群雄角色可在他们各自的出牌阶段给你一张【闪】或【闪电】，每阶段限一次。 
    /// </summary>
    public class HuangTianGivenSkill : ActiveSkill, IRulerGivenSkill
    {
        public override VerifierResult Validate(GameEventArgs arg)
        {
            PlayerAttribute HuangTianUsed = PlayerAttribute.Register("HuangTianUsed" + Master.Id, true);
            if (Owner[HuangTianUsed] != 0)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets != null && arg.Targets.Count > 0)
            {
                return VerifierResult.Fail;
            }
            List<Card> cards = arg.Cards;
            if (cards != null && cards.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (cards != null && cards.Count > 0 && !(cards[0].Type is Shan || cards[0].Type is ShanDian))
            {
                return VerifierResult.Fail;
            }
            if (cards == null || cards.Count == 0)
            {
                return VerifierResult.Partial;
            }
            return VerifierResult.Success;
        }

        public override bool Commit(GameEventArgs arg)
        {
            PlayerAttribute HuangTianUsed = PlayerAttribute.Register("HuangTianUsed"+Master.Id, true);
            Owner[HuangTianUsed] = 1;
            Game.CurrentGame.HandleCardTransferToHand(Owner, Master, arg.Cards);
            return true;
        }


        public override void CardRevealPolicy(Player p, List<Card> cards, List<Player> players)
        {
            foreach (Card c in cards)
            {
                c.RevealOnce = true;
            }
        }

        public Player Master { get; set; }
    }

    public class HuangTian : RulerGivenSkillContainerSkill
    {
        public HuangTian() : base(new HuangTianGivenSkill(), Allegiance.Qun)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Expansions.Battle.Cards;
using System.Diagnostics;
using Sanguosha.Core.Players;

namespace Sanguosha.Expansions.Hills.Skills
{
    /// <summary>
    /// 龙魂-你可以将同花色的x张牌按下列规则使用(或打出)：红桃当桃，方块当火杀，梅花当闪，黑桃当无懈可击。(x为你的当前体力值，且至少为1)
    /// </summary>
    public class LongHun : CardTransformSkill
    {
        protected override int GenerateSpecialEffectHintIndex(Core.Players.Player source, List<Core.Players.Player> targets, CompositeCard card)
        {
            return (int)card.Subcards[0].Suit - 1;
        }

        public override List<CardHandler> PossibleResults
        {
            get { return new List<CardHandler>() { new Tao(), new HuoSha(), new Shan(), new WuXieKeJi()}; }
        }

        public override VerifierResult TryTransform(List<Card> cards, List<Player> arg, out CompositeCard card)
        {
            card = null;
            if (cards == null || cards.Count == 0)
            {
                return VerifierResult.Partial;
            }
            Trace.Assert(Owner != null);
            if (Owner == null) return VerifierResult.Fail;
            int X = Math.Max(Owner.Health, 1);
            if (cards.Count > X)
            {
                return VerifierResult.Fail;
            }
            foreach (var cc in cards)
            {
                if (cc.Owner != Owner || cc.Suit != cards[0].Suit)
                {
                    return VerifierResult.Fail;
                }
            }
            if (cards.Count < X)
            {
                return VerifierResult.Partial;
            }
            card = new CompositeCard();
            card.Subcards = new List<Card>(cards);
            if (cards[0].Suit == SuitType.Heart)
            {
                card.Type = new Tao();
            }
            if (cards[0].Suit == SuitType.Diamond)
            {
                card.Type = new HuoSha();
            }
            if (cards[0].Suit == SuitType.Club)
            {
                card.Type = new Shan();
            }
            if (cards[0].Suit == SuitType.Spade)
            {
                card.Type = new WuXieKeJi();
            }
            return VerifierResult.Success;
        }
    }
}

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
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Expansions.Battle.Cards;

namespace Sanguosha.Expansions.OverKnightFame11.Skills
{
    /// <summary>
    /// 酒诗―若你的武将牌正面朝上，你可以(在合理的时机)将你的武将牌翻面来视为使用一张【酒】；当你的武将牌背面朝上时你受到伤害，你可在伤害结算后将之翻回正面。
    /// </summary>
    public class JiuShi : CardTransformSkill
    {
        public class JiuShiPassive : TriggerSkill
        {
            public static CardAttribute JiuShiUsable = CardAttribute.Register("JiuShiUsable");
            public JiuShiPassive()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return (a as DamageEventArgs).ReadonlyCard[JiuShiUsable] == 1; },
                    (p, e, a) => { (a as DamageEventArgs).ReadonlyCard[JiuShiUsable] = 0; p.IsImprisoned = false; },
                    TriggerCondition.OwnerIsTarget
                ) { };
                Triggers.Add(GameEvent.DamageComputingFinished, trigger);
                var trigger2 = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return p.IsImprisoned; },
                    (p, e, a) => { (a as DamageEventArgs).ReadonlyCard[JiuShiUsable] = 1; },
                    TriggerCondition.OwnerIsTarget
                ) { IsAutoNotify = false, AskForConfirmation = false, Priority = int.MinValue };
                Triggers.Add(GameEvent.DamageInflicted, trigger2);
                IsAutoInvoked = null;
            }
        }

        public JiuShi()
        {
            Helper.HasNoConfirmation = true;
            LinkedPassiveSkill = new JiuShiPassive();
        }

        public override VerifierResult TryTransform(List<Card> cards, List<Player> arg, out CompositeCard card, bool isPlay)
        {
            card = new CompositeCard();
            card.Type = new Jiu();
            if (Owner.IsImprisoned)
            {
                return VerifierResult.Fail;
            }
            if (cards != null && cards.Count != 0)
            {
                return VerifierResult.Fail;
            }

            return VerifierResult.Success;
        }

        protected override bool DoTransformSideEffect(CompositeCard card, object arg, List<Player> targets, bool isPlay)
        {
            Owner.IsImprisoned = true;
            return true;
        }

        public override List<CardHandler> PossibleResults
        {
            get { return new List<CardHandler>() { new Jiu() }; }
        }

    }
}

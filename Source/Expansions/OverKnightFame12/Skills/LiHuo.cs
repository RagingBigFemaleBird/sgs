using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Expansions.Battle.Cards;
using System.Diagnostics;

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    /// <summary>
    /// 疠火–你可以将一张普通【杀】当火【杀】使用，若以此法使用的【杀】造成了伤害，在此【杀】结算后你失去1点体力；你使用火【杀】时，可以额外选择一个目标。
    /// </summary>
    public class LiHuo : CardTransformSkill
    {
        public LiHuo()
        {
            LinkedPassiveSkill = new LiHuoPassive();
        }
        public override VerifierResult TryTransform(List<Card> cards, object arg, out CompositeCard card)
        {
            card = null;
            if (cards == null || cards.Count < 1)
            {
                return VerifierResult.Partial;
            }
            if (cards.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (cards[0].Owner != Owner || cards[0].Place.DeckType != DeckType.Hand)
            {
                return VerifierResult.Fail;
            }
            if (cards[0].Type is RegularSha)
            {
                card = new CompositeCard();
                card.Subcards = new List<Card>(cards);
                card.Type = new HuoSha();
                card[LiHuoSha] = 1;
                return VerifierResult.Success;
            }
            return VerifierResult.Fail;
        }

        public override List<CardHandler> PossibleResults { get { return new List<CardHandler>() { new HuoSha()}; } }

        public class LiHuoPassive : TriggerSkill
        {
            public LiHuoPassive()
            {
                var trigger1 = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return a.ReadonlyCard != null && a.ReadonlyCard[LiHuoSha] != 0; },
                    (p, e, a) =>
                    {
                        a.ReadonlyCard[LiHuoShaCausedDamage] = 1;
                    },
                    TriggerCondition.OwnerIsSource
                ) { AskForConfirmation = false, IsAutoNotify = false };

                var trigger2 = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return a.Card.Type is HuoSha; },
                    (p, e, a) =>
                    {
                        ShaEventArgs args = (ShaEventArgs)a;
                        Trace.Assert(args != null);
                        if (args.Source != Owner) return;
                        if (args.TargetApproval[0])
                        {
                            for (int i = 1; i < args.TargetApproval.Count; i++)
                            {
                                if (!args.TargetApproval[i])
                                {
                                    args.TargetApproval[i] = true;
                                    break;
                                }
                            }
                        }
                    },
                    TriggerCondition.OwnerIsSource
                ) { AskForConfirmation = false, IsAutoNotify = false };

                var trigger3 = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return a.ReadonlyCard != null && a.ReadonlyCard[LiHuoShaCausedDamage] != 0; },
                    (p, e, a) =>
                    {
                        Game.CurrentGame.LoseHealth(p, 1);
                    },
                    TriggerCondition.OwnerIsSource
                ) { AskForConfirmation = false };

                Triggers.Add(GameEvent.AfterDamageCaused, trigger1);
                Triggers.Add(Sha.PlayerShaTargetValidation, trigger2);
                Triggers.Add(GameEvent.CardUsageDone, trigger3);
            }
        }

        private static CardAttribute LiHuoSha = CardAttribute.Register("LiHuoSha");
        private static CardAttribute LiHuoShaCausedDamage = CardAttribute.Register("LiHuoShaCausedDamage");
    }
}

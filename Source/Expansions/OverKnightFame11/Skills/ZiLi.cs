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
    /// <summary>
    /// 自立-觉醒技，回合开始阶段开始时，若“权”的数量达到3或更多，你须减1点体力上限并选择一项：回复1点体力，或摸两张牌，然后获得技能“排异”（出牌阶段，你可以将一张“权”置入弃牌堆，令一名角色摸两张牌，然后若该角色的手牌数大于你的手牌数，你对其造成1点伤害。每阶段限一次）。
    /// </summary>
    public class ZiLi : TriggerSkill
    {
        public ZiLi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return p[ZiLiAwakened] == 0 && Game.CurrentGame.Decks[p, QuanJi.QuanDeck].Count >= 3; },
                (p, e, a) => 
                { 
                    p[ZiLiAwakened] = 1;
                    Game.CurrentGame.LoseMaxHealth(p, 1);
                    int answer = 0;
                    Owner.AskForMultipleChoice(
                            new MultipleChoicePrompt("ZiLi"),
                            new List<OptionPrompt>() { new OptionPrompt("ZiLiRecover"), new OptionPrompt("ZiLiDraw") },
                            out answer);
                    if (answer == 0)
                    {
                        Game.CurrentGame.RecoverHealth(Owner, Owner, 1);
                    }
                    else
                    {
                        Game.CurrentGame.DrawCards(Owner, 2);
                    }
                    Game.CurrentGame.PlayerAcquireAdditionalSkill(Owner, new PaiYi(), HeroTag); 
                },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Start], trigger);
            IsAwakening = true;
        }

        public class PaiYi : ActiveSkill
        {
            public PaiYi()
            {
                Helper.OtherDecksUsed.Add(QuanJi.QuanDeck);
            }

            public override VerifierResult Validate(GameEventArgs arg)
            {
                if (Owner[PaiYiUsed] != 0)
                    return VerifierResult.Fail;
                List<Card> cards = arg.Cards;
                if (arg.Targets != null && arg.Targets.Count != 0 && arg.Targets.Count > 1)
                {
                    return VerifierResult.Fail;
                }
                if (cards != null && cards.Count() > 1)
                {
                    return VerifierResult.Fail;
                }
                if (cards != null && cards.Count == 1 && cards[0].Place.DeckType != QuanJi.QuanDeck)
                {
                    return VerifierResult.Fail;
                }
                if (cards == null || cards.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (arg.Targets == null || arg.Targets.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                return VerifierResult.Success;
            }

            public override bool Commit(GameEventArgs arg)
            {
                Trace.Assert(arg.Cards.Count == 1 && arg.Targets.Count == 1);
                Owner[PaiYiUsed] = 1;
                Player target = arg.Targets.First();
                Game.CurrentGame.HandleCardDiscard(Owner, arg.Cards);
                Game.CurrentGame.DrawCards(target, 2);
                if (target.HandCards().Count > Owner.HandCards().Count)
                {
                    Game.CurrentGame.DoDamage(Owner, target, 1, DamageElement.None, null, null);
                }
                return true;
            }

        }

        public static PlayerAttribute ZiLiAwakened = PlayerAttribute.Register("ZiLiAwakened");
        public static PlayerAttribute PaiYiUsed = PlayerAttribute.Register("PaiYiUsed", true);
    }
}

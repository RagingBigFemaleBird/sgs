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
using Sanguosha.Expansions.Basic.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    /// <summary>
    /// 父魂-你可以将两张手牌当【杀】使用；出牌阶段，若你以此法使用的【杀】造成了伤害，你获得技能“武圣”、“咆哮”，直到回合结束。
    /// </summary>
    public class FuHun : CardTransformSkill
    {
        public FuHun()
        {
            LinkedPassiveSkill = new FuHunPassiveSkill();
        }

        public override VerifierResult TryTransform(List<Card> cards, object arg, out CompositeCard card)
        {
            card = null;
            if (cards != null && (cards.Count > 2 || cards.Any(c => c.Place.DeckType != DeckType.Hand)))
            {
                return VerifierResult.Fail;
            }
            if (cards == null || cards.Count < 2)
            {
                return VerifierResult.Partial;
            }
            card = new CompositeCard(cards);
            card.Type = new RegularSha();
            card.Owner = Owner;
            card[FuHunSha] = 1;
            return VerifierResult.Success;
        }

        public override List<CardHandler> PossibleResults
        {
            get
            {
                return new List<CardHandler>() { new RegularSha() };
            }
        }

        class FuHunPassiveSkill : TriggerSkill
        {
            public class RemoveShengPao : Trigger
            {
                public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
                {
                    if (eventArgs.Source != Owner) return;
                    fuhun.fhPaoXiao = null;
                    fuhun.fhWuSheng = null;
                    Game.CurrentGame.PlayerLoseAdditionalSkill(Owner, fhWuSheng);
                    Game.CurrentGame.PlayerLoseAdditionalSkill(Owner, fhPaoXiao);
                    Game.CurrentGame.UnregisterTrigger(GameEvent.PhasePostEnd, this);
                }

                FuHunPassiveSkill fuhun;
                public RemoveShengPao(Player p, FuHunPassiveSkill fuhun, ISkill s1, ISkill s2)
                {
                    Owner = p;
                    fhWuSheng = s1;
                    fhPaoXiao = s2;
                    this.fuhun = fuhun;
                }
                ISkill fhWuSheng;
                ISkill fhPaoXiao;
            }

            ISkill fhWuSheng;
            ISkill fhPaoXiao;
            public FuHunPassiveSkill()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return fhWuSheng == null && a.ReadonlyCard != null && a.ReadonlyCard[FuHunSha] == 1 && Game.CurrentGame.PhasesOwner == p && Game.CurrentGame.CurrentPhase == TurnPhase.Play; },
                    (p, e, a) =>
                    {
                        fhWuSheng = new WuSheng();
                        fhPaoXiao = new PaoXiao();
                        Trigger tri = new RemoveShengPao(p, this, fhWuSheng, fhPaoXiao);
                        Game.CurrentGame.PlayerAcquireAdditionalSkill(p, fhWuSheng, HeroTag);
                        Game.CurrentGame.PlayerAcquireAdditionalSkill(p, fhPaoXiao, HeroTag);
                        Game.CurrentGame.RegisterTrigger(GameEvent.PhasePostEnd, tri);
                    },
                    TriggerCondition.OwnerIsSource
                ) { AskForConfirmation = false };
                Triggers.Add(GameEvent.AfterDamageCaused, trigger);

                var trigger2 = new AutoNotifyPassiveSkillTrigger(
                    this,
                    (p, e, a) => { return a.Card[FuHunSha] == 1; },
                    (p, e, a) => { throw new TriggerResultException(TriggerResult.Fail); },
                    TriggerCondition.OwnerIsSource
                ) { AskForConfirmation = false, IsAutoNotify = false };
                Triggers.Add(GameEvent.PlayerCanPlayCard, trigger2);
            }
        }

        private static CardAttribute FuHunSha = CardAttribute.Register("FuHunSha");
    }
}

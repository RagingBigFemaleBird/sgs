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
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Assassin.Skills
{
    /// <summary>
    /// 竭缘-每当你对一名当前的体力值不比你少的其他角色造成伤害时，你可以弃置一张黑色手牌，令此伤害+1；每当你受到一名当前的体力值不比你少的其他角色对你造成的伤害时，你可以弃置一张红色手牌，令此伤害-1。
    /// </summary>
    public class JieYuan : TriggerSkill
    {
        protected override int GenerateSpecialEffectHintIndex(Player source, List<Player> targets)
        {
            return JieYuanEffect;
        }

        class JieYuanVerifier : CardsAndTargetsVerifier
        {
            bool isDamageInflicted;
            public JieYuanVerifier(bool damageInflicted)
            {
                MaxCards = 1;
                MinCards = 1;
                MaxPlayers = 0;
                Discarding = true;
                isDamageInflicted = damageInflicted;
            }
            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Place.DeckType == DeckType.Hand && card.SuitColor == (isDamageInflicted ? SuitColorType.Red : SuitColorType.Black);
            }
        }

        int JieYuanEffect;
        public JieYuan()
        {
            var trigger = new AutoNotifyUsagePassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Source != null && p.Health <= a.Source.Health && p != a.Source; },
                (p, e, a, c, pls) =>
                {
                    JieYuanEffect = 0;
                    NotifySkillUse();
                    Game.CurrentGame.HandleCardDiscard(p, c);
                    var damageArgs = a as DamageEventArgs;
                    damageArgs.Magnitude--;
                    if (damageArgs.Magnitude == 0) throw new TriggerResultException(TriggerResult.End);
                },
                TriggerCondition.OwnerIsTarget,
                new JieYuanVerifier(true)
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.DamageInflicted, trigger);

            var trigger2 = new AutoNotifyUsagePassiveSkillTrigger(
                this,
                (p, e, a) => { return p.Health <= a.Targets[0].Health && p != a.Targets[0]; },
                (p, e, a, c, pls) =>
                {
                    JieYuanEffect = 1;
                    NotifySkillUse();
                    Game.CurrentGame.HandleCardDiscard(p, c);
                    var damageArgs = a as DamageEventArgs;
                    damageArgs.Magnitude++;
                },
                TriggerCondition.OwnerIsSource,
                new JieYuanVerifier(false)
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.DamageCaused, trigger2);
            IsAutoInvoked = null;
        }
    }
}

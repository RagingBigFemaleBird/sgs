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
using Sanguosha.Expansions.Basic.Skills;
using Sanguosha.Expansions.Woods.Skills;

namespace Sanguosha.Expansions.Hills.Skills
{
    /// <summary>
    /// 凿险-觉醒技，回合开始阶段开始时，若"田"的数量达到3张或更多，你须减1点体力上限，并获得技能“急袭”(出牌阶段，你可以把一张"田"当【顺手牵羊】使用)。
    /// </summary>
    public class ZaoXian : TriggerSkill
    {
        public ZaoXian()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return p[ZaoXianAwakened] == 0 && Game.CurrentGame.Decks[p, TunTian.TianDeck].Count >= 3; },
                (p, e, a) => { p[ZaoXianAwakened] = 1; Game.CurrentGame.LoseMaxHealth(p, 1); Game.CurrentGame.PlayerAcquireAdditionalSkill(p, new JiXi(), HeroTag); },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Start], trigger);
            IsAwakening = true;
        }
        public static PlayerAttribute ZaoXianAwakened = PlayerAttribute.Register("ZaoXianAwakened");

        public class JiXi : OneToOneCardTransformSkill
        {
            public override bool VerifyInput(Card card, object arg)
            {
                return card.Place.Player == Owner && card.Place.DeckType == TunTian.TianDeck;
            }

            public override CardHandler PossibleResult
            {
                get { return new ShunShouQianYang(); }
            }

            public JiXi()
            {
                Helper.OtherDecksUsed.Add(TunTian.TianDeck);
            }
        }
    }
}

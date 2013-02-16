using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Skills;
using Sanguosha.Expansions.Hills.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.StarSP.Skills
{
    /// <summary>
    /// 谋断-转化技，通常状态下，你拥有标记“武”并拥有技能“激昂”和“谦逊”。当你的手牌数为2张或以下时，你须将你的标记翻面为“文”，将该两项技能转化为“英姿”和“克己”。任一角色的回合开始前，你可弃一张牌将标记翻回。
    /// </summary>
    class MouDuan : TriggerSkill
    {
        class MouDuanVerifier : CardsAndTargetsVerifier
        {
            public MouDuanVerifier()
            {
                MaxCards = 1;
                MinCards = 1;
                MaxPlayers = 0;
                Discarding = true;
            }
        }

        void loseMouDuan(Player Owner)
        {
            if (Owner[WuMark] != 0)
            {
                Owner[WuMark] = 0;
                Game.CurrentGame.PlayerLoseAdditionalSkill(Owner, mdJiAng);
                Game.CurrentGame.PlayerLoseAdditionalSkill(Owner, mdQianXun);
            }
            else
            {
                Owner[WenMark] = 0;
                Game.CurrentGame.PlayerLoseAdditionalSkill(Owner, mdYingZi);
                Game.CurrentGame.PlayerLoseAdditionalSkill(Owner, mdKeJi);
            }
        }

        public override Player Owner
        {
            get
            {
                return base.Owner;
            }
            set
            {
                Player original = base.Owner;
                base.Owner = value;
                if (base.Owner == null && original != null)
                {
                    loseMouDuan(original);
                }
            }
        }

        public MouDuan()
        {
            mdJiAng = new JiAng();
            mdQianXun = new QianXun();
            mdYingZi = new YingZi();
            mdKeJi = new KeJi();

            var trigger1 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    if (e == GameEvent.PlayerSkillSetChanged)
                    {
                        SkillSetChangedEventArgs args = a as SkillSetChangedEventArgs;
                        return !args.IsLosingSkill && args.Skills.Contains(this);
                    }
                    return true;
                },
                (p, e, a) =>
                {
                    if (p.HandCards().Count > 2)
                    {
                        p[WuMark] = 1;
                        Game.CurrentGame.PlayerAcquireAdditionalSkill(p, mdJiAng, HeroTag);
                        Game.CurrentGame.PlayerAcquireAdditionalSkill(p, mdQianXun, HeroTag);
                    }
                    else
                    {
                        p[WenMark] = 1;
                        Game.CurrentGame.PlayerAcquireAdditionalSkill(p, mdYingZi, HeroTag);
                        Game.CurrentGame.PlayerAcquireAdditionalSkill(p, mdKeJi, HeroTag);
                    }
                },
                TriggerCondition.OwnerIsSource
                ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PlayerGameStartAction, trigger1);
            Triggers.Add(GameEvent.PlayerSkillSetChanged, trigger1);

            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return p.HandCards().Count <= 2 && p[WuMark] != 0; },
                (p, e, a) =>
                {
                    p[WuMark] = 0;
                    p[WenMark] = 1;
                    Game.CurrentGame.PlayerLoseAdditionalSkill(p, mdJiAng);
                    Game.CurrentGame.PlayerLoseAdditionalSkill(p, mdQianXun);
                    Game.CurrentGame.PlayerAcquireAdditionalSkill(p, mdYingZi, HeroTag);
                    Game.CurrentGame.PlayerAcquireAdditionalSkill(p, mdKeJi, HeroTag);
                },
                TriggerCondition.OwnerIsSource
                ) { AskForConfirmation = false };
            Triggers.Add(GameEvent.CardsLost, trigger2);

            var trigger3 = new AutoNotifyUsagePassiveSkillTrigger(
                this,
                (p, e, a) => { return p[WenMark] != 0; },
                (p, e, a, c, t) =>
                {
                    Game.CurrentGame.HandleCardDiscard(p, c);
                    if (p.HandCards().Count <= 2) return;
                    p[WuMark] = 1;
                    p[WenMark] = 0;
                    Game.CurrentGame.PlayerLoseAdditionalSkill(p, mdYingZi);
                    Game.CurrentGame.PlayerLoseAdditionalSkill(p, mdKeJi);
                    Game.CurrentGame.PlayerAcquireAdditionalSkill(p, mdJiAng, HeroTag);
                    Game.CurrentGame.PlayerAcquireAdditionalSkill(p, mdQianXun, HeroTag);
                },
                TriggerCondition.Global,
                new MouDuanVerifier()
                ) { AskForConfirmation = false };
            Triggers.Add(GameEvent.PhaseBeforeStart, trigger3);

            IsAutoInvoked = null;
        }

        private ISkill mdJiAng;
        private ISkill mdQianXun;
        private ISkill mdYingZi;
        private ISkill mdKeJi;
        private static PlayerAttribute WuMark = PlayerAttribute.Register("Wu", false, true);
        private static PlayerAttribute WenMark = PlayerAttribute.Register("Wen", false, true);
    }
}

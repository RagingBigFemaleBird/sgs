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
using Sanguosha.Expansions.Hills.Skills;
using Sanguosha.Expansions.Basic.Skills;
using Sanguosha.Expansions.Wind.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.SP.Skills
{
    /// <summary>    
    /// 豹变-锁定技，若你的体力值为3或更少，你视为拥用技能“挑衅”；若你的体力值为2或更少，你视为拥有技能“咆哮”；若你的体力值为1，你视为拥有技能“神速”。
    /// </summary>
    public class BaoBian : TriggerSkill
    {
        PaoXiao bbPaoXiao;
        ShenSu bbShenSu;
        TiaoXin bbTiaoXin;
        void Refresh(Player p)
        {
            if (p.Health <= 3)
            {
                if (bbTiaoXin == null)
                {
                    bbTiaoXin = new TiaoXin();
                    Game.CurrentGame.PlayerAcquireAdditionalSkill(p, bbTiaoXin, HeroTag);
                }
            }
            else
            {
                if (bbTiaoXin != null)
                {
                    Game.CurrentGame.PlayerLoseAdditionalSkill(p, bbTiaoXin);
                    bbTiaoXin = null;
                }
            }
            if (p.Health <= 2)
            {
                if (bbPaoXiao == null)
                {
                    bbPaoXiao = new PaoXiao();
                    Game.CurrentGame.PlayerAcquireAdditionalSkill(p, bbPaoXiao, HeroTag);
                }
            }
            else
            {
                if (bbPaoXiao != null)
                {
                    Game.CurrentGame.PlayerLoseAdditionalSkill(p, bbPaoXiao);
                    bbPaoXiao = null;
                }
            }
            if (p.Health == 1)
            {
                if (bbShenSu == null)
                {
                    bbShenSu = new ShenSu();
                    Game.CurrentGame.PlayerAcquireAdditionalSkill(p, bbShenSu, HeroTag);
                }
            }
            else
            {
                if (bbShenSu != null)
                {
                    Game.CurrentGame.PlayerLoseAdditionalSkill(p, bbShenSu);
                    bbShenSu = null;
                }
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
                if (Owner == value) return;
                if (Owner != null)
                {
                    Game.CurrentGame.PlayerLoseAdditionalSkill(Owner, bbPaoXiao);
                    Game.CurrentGame.PlayerLoseAdditionalSkill(Owner, bbShenSu);
                    Game.CurrentGame.PlayerLoseAdditionalSkill(Owner, bbTiaoXin);
                    bbPaoXiao = null;
                    bbShenSu = null;
                    bbTiaoXin = null;
                }
                base.Owner = value;
                if (Owner != null && Owner.MaxHealth > 0) Refresh(Owner);
            }
        }
        public BaoBian()
        {
            bbPaoXiao = null;
            bbShenSu = null;
            bbTiaoXin = null;
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                    (p, e, a) => { Refresh(p); },
                    TriggerCondition.OwnerIsTarget
                ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.AfterHealthChanged, trigger);
            IsEnforced = true;
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                    (p, e, a) => { Refresh(p); },
                    TriggerCondition.OwnerIsSource
                ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.PlayerGameStartAction, trigger2);
         }
    }
}

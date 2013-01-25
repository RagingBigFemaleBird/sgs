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

namespace Sanguosha.Expansions.SP.Skills
{
    /// <summary>    
    /// 豹变-锁定技，若你的体力值为3或更少，你视为拥用技能“挑衅”；若你的体力值为2或更少，你视为拥有技能“咆哮”；若你的体力值为1，你视为拥有技能“神速”。
    /// </summary>
    public class BaoBian : TriggerSkill
    {
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
                    foreach (var skill in skills.Values)
                    {
                        Game.CurrentGame.PlayerLoseSkill(original, skill);
                    }
                }
            }
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            bool getSkills = false;
            foreach (int health in skills.Keys)
            {
                if (Owner.Health <= health && !Owner.AdditionalSkills.Contains(skills[health]))
                {
                    getSkills = true;
                    Game.CurrentGame.PlayerAcquireSkill(Owner, skills[health]);
                }
                if (Owner.Health > health) Game.CurrentGame.PlayerLoseSkill(Owner, skills[health]);
            }
            if (Owner.Health < 1) Game.CurrentGame.PlayerLoseSkill(Owner, bbShenSu);
            if (getSkills) NotifySkillUse();
        }

        public BaoBian()
        {
            bbTiaoXin = new TiaoXin();
            bbPaoXiao = new PaoXiao();
            bbShenSu = new ShenSu();
            skills = new Dictionary<int, ISkill>();
            skills[3] = bbTiaoXin;
            skills[2] = bbPaoXiao;
            skills[1] = bbShenSu;
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    if (e == GameEvent.AfterHealthChanged) return a.Targets.Contains(p);
                    return a.Source == p;
                },
                Run,
                TriggerCondition.Global
            ) { IsAutoNotify = false, Priority = 10 };
            Triggers.Add(GameEvent.AfterHealthChanged, trigger);
            Triggers.Add(GameEvent.PlayerGameStartAction, trigger);

            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    var arg = a as SkillSetChangedEventArgs;
                    return !arg.IsLosingSkill && arg.Skills.Contains(this);
                },
                Run,
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.PlayerSkillSetChanged, trigger2);

            IsEnforced = true;
        }

        Dictionary<int, ISkill> skills;
        ISkill bbTiaoXin;
        ISkill bbPaoXiao;
        ISkill bbShenSu;
    }
}

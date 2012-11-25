using Sanguosha.Core.Games;
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Skills
{
    public class RulerGivenSkillContainerSkill : TriggerSkill
    {
        private Allegiance allegiance;

        public Allegiance Allegiance
        {
            get { return allegiance; }
            protected set { allegiance = value; }
        }

        protected void DistributeSkills(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            foreach (var player in Game.CurrentGame.AlivePlayers)
            {
                if (player.Allegiance == Allegiance && owner != player && !masterList.ContainsKey(player))
                {
                    IRulerGivenSkill skill = Activator.CreateInstance(innerSkillType) as IRulerGivenSkill;
                    Trace.Assert(skill != null);
                    skill.Master = owner;
                    masterList.Add(player, skill);
                    player.AcquireAdditionalSkill(skill);
                }
            }
        }

        protected void RevokeAllSkills()
        {
            foreach (var pair in masterList)
            {
                pair.Key.LoseAdditionalSkill(pair.Value);
            }
            masterList.Clear();
        }

        protected Dictionary<Player, IRulerGivenSkill> masterList;
        protected Type innerSkillType;
        public RulerGivenSkillContainerSkill(IRulerGivenSkill InnerSkill, Allegiance al)
        {
            innerSkillType = InnerSkill.GetType();
            Allegiance = al;
            masterList = new Dictionary<Player, IRulerGivenSkill>();
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                DistributeSkills,
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false, AskForConfirmation = false };
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => {return masterList.ContainsKey(a.Source);},
                (p, e, a) =>
                {
                    if (a.Source.Allegiance != Allegiance)
                    {
                        ISkill skill = masterList[a.Source];
                        masterList.Remove(a.Source);
                        a.Source.LoseAdditionalSkill(skill);
                    }
                },
                TriggerCondition.Global
            ) { IsAutoNotify = false, AskForConfirmation = false };
            Triggers.Add(GameEvent.PlayerGameStartAction, trigger);
            Triggers.Add(GameEvent.PlayerChangedAllegiance, trigger2);
            IsAutoInvoked = null;
        }

        public override Player Owner
        {
            get
            {
                return base.Owner;
            }
            set
            {
                if (base.Owner == null && value != null)
                {
                    base.Owner = value;
                    DistributeSkills(base.Owner, null, null);
                }
                else
                {
                    base.Owner = value;
                    foreach (var pair in masterList)
                    {
                        pair.Value.Master = base.Owner;
                    }
                }
                if (value == null) RevokeAllSkills();
            }
        }

        public override bool IsRulerOnly
        {
            get
            {
                return true;
            }
        }
    }
}

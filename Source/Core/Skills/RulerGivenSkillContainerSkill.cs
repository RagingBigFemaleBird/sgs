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
        private Allegiance? qualifiedAllegiance;

        public Allegiance? QualifiedAllegiance
        {
            get { return qualifiedAllegiance; }
            protected set { qualifiedAllegiance = value; }
        }

        protected void DistributeSkills(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            foreach (var player in Game.CurrentGame.AlivePlayers)
            {
                if ((QualifiedAllegiance == null || player.Allegiance == QualifiedAllegiance) && owner != player && !masterList.ContainsKey(player))
                {
                    IRulerGivenSkill skill = Activator.CreateInstance(innerSkillType) as IRulerGivenSkill;
                    Trace.Assert(skill != null);
                    skill.Master = owner;
                    masterList.Add(player, skill);
                    Game.CurrentGame.PlayerAcquireAdditionalSkill(player, skill, HeroTag, true);
                }
            }
        }

        protected void RevokeAllSkills()
        {
            foreach (var pair in masterList)
            {
                Game.CurrentGame.PlayerLoseAdditionalSkill(pair.Key, pair.Value, true);
            }
            masterList.Clear();
        }

        protected Dictionary<Player, IRulerGivenSkill> masterList;
        protected Type innerSkillType;
        public RulerGivenSkillContainerSkill(IRulerGivenSkill InnerSkill, Allegiance? al, bool NotARuler = false)
        {
            innerSkillType = InnerSkill.GetType();
            QualifiedAllegiance = al;
            masterList = new Dictionary<Player, IRulerGivenSkill>();
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                DistributeSkills,
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false, AskForConfirmation = false };
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    if ((QualifiedAllegiance == null || a.Source.Allegiance == QualifiedAllegiance) && !masterList.ContainsKey(a.Source))
                    {
                        DistributeSkills(Owner, null, null);
                    }
                    if (QualifiedAllegiance != null && a.Source.Allegiance != QualifiedAllegiance && masterList.ContainsKey(a.Source))
                    {
                        ISkill skill = masterList[a.Source];
                        masterList.Remove(a.Source);
                        Game.CurrentGame.PlayerLoseAdditionalSkill(a.Source, skill, true);
                    }
                },
                TriggerCondition.Global
            ) { IsAutoNotify = false, AskForConfirmation = false };
            Triggers.Add(GameEvent.PlayerGameStartAction, trigger);
            Triggers.Add(GameEvent.PlayerChangedAllegiance, trigger2);
            IsAutoInvoked = null;
            IsRulerOnly = !NotARuler;
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

    }
}

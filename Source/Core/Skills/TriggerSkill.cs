using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Games;
using System.Diagnostics;
using Sanguosha.Core.Players;


namespace Sanguosha.Core.Skills
{
    public abstract class TriggerSkill : PassiveSkill
    {
        public TriggerSkill()
        {
            Triggers = new Dictionary<GameEvent, Trigger>();
        }

        protected class AutoNotifyPassiveSkillTrigger : Trigger
        {
            public AutoNotifyPassiveSkillTrigger(TriggerSkill skill, TriggerPredicate canExecute, TriggerAction execute, TriggerCondition condition) :
                this(skill, new RelayTrigger(canExecute, execute, condition))
            {}

            public AutoNotifyPassiveSkillTrigger(TriggerSkill skill, TriggerAction execute, TriggerCondition condition) :
                this(skill, new RelayTrigger(execute, condition))
            { }

            public AutoNotifyPassiveSkillTrigger(TriggerSkill skill, RelayTrigger innerTrigger)
            {
                Skill = skill;
                InnerTrigger = innerTrigger;
                base.Owner = InnerTrigger.Owner;
            }

            public override Player Owner
            {
                get
                {
                    return base.Owner;
                }
                set
                {
                    base.Owner = value;
                    if (InnerTrigger.Owner != value)
                    {
                        InnerTrigger.Owner = value;
                    }
                }
            }

            protected bool AskForSkillUse()
            {
                int answer;
                return (Game.CurrentGame.UiProxies[Owner].AskForMultipleChoice(
                        new MultipleChoicePrompt(Prompt.SkillUseYewNoPrompt, Skill), Prompt.YesNoChoices, out answer)
                        && answer == 0);
            }

            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (InnerTrigger.CanExecute(Owner, gameEvent, eventArgs))
                {
                    if (!Skill.IsEnforced && !AskForSkillUse())
                    {
                        return;
                    }

                    ActionLog log = new ActionLog();
                    log.GameAction = GameAction.None;
                    log.SkillAction = Skill;
                    log.Source = Owner;
                    Games.Game.CurrentGame.NotificationProxy.NotifySkillUse(log);
                    InnerTrigger.Execute(Owner, gameEvent, eventArgs);
                }
            }

            public TriggerSkill Skill
            {
                get;
                set;
            }

            public RelayTrigger InnerTrigger
            {
                get;
                set;
            }
        }
        
        private bool _isTriggerInstalled;

        protected override void InstallTriggers(Players.Player owner)
        {
            Trace.Assert(!_isTriggerInstalled,
                string.Format("Trigger already installed for skill {0}", this.GetType().FullName));
            foreach (var pair in Triggers)
            {
                pair.Value.Owner = owner;
                Game.CurrentGame.RegisterTrigger(pair.Key, pair.Value);
            }
            _isTriggerInstalled = true;
        }

        protected override void UninstallTriggers(Players.Player owner)
        {
            Trace.Assert(_isTriggerInstalled,
                string.Format("Trigger not installed yet for skill {0}", this.GetType().FullName));
            foreach (var pair in Triggers)
            {
                pair.Value.Owner = null;
                Game.CurrentGame.UnregisterTrigger(pair.Key, pair.Value);
            }
        }

        protected IDictionary<GameEvent, Trigger> Triggers
        {
            get;
            private set;
        }

    }
}

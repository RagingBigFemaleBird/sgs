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

    public abstract class UsageTriggerSkill : PassiveSkill
    {
        public UsageTriggerSkill()
        {
            Triggers = new Dictionary<GameEvent, Trigger>();
        }

        public void NotifySkillUse(List<Player> targets)
        {
            ActionLog log = new ActionLog();
            log.GameAction = GameAction.None;
            log.SkillAction = this;
            log.Source = Owner;
            log.Targets = targets;
            Games.Game.CurrentGame.NotificationProxy.NotifySkillUse(log);
        }

        protected abstract ICardUsageVerifier GetVerifier();

        protected bool AskForSkillUse(out List<Card> cards, out List<Player> players)
        {
            ISkill skill;
            var ret = Game.CurrentGame.UiProxies[Owner].AskForCardUsage(
                    new CardUsagePrompt(this.GetType().Name, this), GetVerifier(), out skill, out cards, out players);
            Trace.Assert(skill == null);
            return ret;
        }


        protected class AutoNotifyUsagePassiveSkillTrigger : Trigger
        {
            public AutoNotifyUsagePassiveSkillTrigger(UsageTriggerSkill skill, TriggerPredicate canExecute, TriggerAction execute, TriggerCondition condition) :
                this(skill, new RelayTrigger(canExecute, execute, condition))
            { }

            public AutoNotifyUsagePassiveSkillTrigger(UsageTriggerSkill skill, TriggerAction execute, TriggerCondition condition) :
                this(skill, new RelayTrigger(execute, condition))
            { }

            public AutoNotifyUsagePassiveSkillTrigger(UsageTriggerSkill skill, RelayTrigger innerTrigger)
            {
                AskForConfirmation = null;
                IsAutoNotify = true;
                Skill = skill;
                InnerTrigger = innerTrigger;
                base.Owner = InnerTrigger.Owner;
            }

            public bool IsAutoNotify { get; set; }
            public bool? AskForConfirmation { get; set; }

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

            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (!InnerTrigger.CheckConditions(gameEvent, eventArgs))
                {
                    return;
                }
                if (InnerTrigger.CanExecute(Owner, gameEvent, eventArgs))
                {
                    List<Card> cards = null;
                    List<Player> players = null;
                    if (((AskForConfirmation == null && !Skill.IsEnforced && !Skill.IsAwakening) || (AskForConfirmation == true)) && !Skill.AskForSkillUse(out cards, out players))
                    {
                        return;
                    }
                    if (IsAutoNotify)
                    {
                        Skill.NotifySkillUse(players);
                    }
                    InnerTrigger.Execute(Owner, gameEvent, eventArgs, cards, players);
                }
            }

            public UsageTriggerSkill Skill
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
            _isTriggerInstalled = false;
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

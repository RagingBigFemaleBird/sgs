using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;
using System.Diagnostics;

namespace Sanguosha.Expansions.Basic.Cards
{
    [Serializable]
    public class QingLongYanYueDao : Weapon
    {
        public QingLongYanYueDao()
        {
            EquipmentSkill = new QingLongYanYueSkill();
        }

        public class QingLongYanYueSkill : TriggerSkill
        {
            protected void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
            {
                ISkill skill;
                List<Card> cards;
                List<Player> players;
                if (Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("QingLongYanYueDao"),
                    new SingleCardUsageVerifier((c) => {return c.Type is Sha;}),
                    out skill, out cards, out players))
                {
                    while (true)
                    {
                        try
                        {
                            NotifySkillUse(new List<Player>());
                            GameEventArgs args = new GameEventArgs();
                            args.Source = eventArgs.Source;
                            args.Targets = eventArgs.Targets;
                            args.Skill = skill;
                            args.Cards = cards;
                            Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
                        }
                        catch (TriggerResultException e)
                        {
                            Trace.Assert(e.Status == TriggerResult.Retry);
                            continue;
                        }
                        break;
                    }
                }
            }
            public QingLongYanYueSkill()
            {
                var trigger = new AutoNotifyPassiveSkillTrigger(
                    this,
                    Run,
                    TriggerCondition.OwnerIsSource
                ) { IsAutoNotify = false, AskForConfirmation = false };
                Triggers.Add(Sha.PlayerShaTargetDodged, trigger);
            }
        }

        public override int AttackRange
        {
            get { return 3; }
        }

        protected override void RegisterWeaponTriggers(Player p)
        {
            return;
        }

        protected override void UnregisterWeaponTriggers(Player p)
        {
            return;
        }

    }
}

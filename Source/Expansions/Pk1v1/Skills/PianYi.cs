using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;
using Sanguosha.Expansions.Pk1v1.Cards;

namespace Sanguosha.Expansions.Pk1v1.Skills
{
    class PianYi : TriggerSkill
    {

        public PianYi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    return Game.CurrentGame.CurrentPlayer != p;
                },
                (p, e, a) => { throw new EndOfTurnException(); },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.HeroDebut, trigger);
            IsEnforced = true;
        }
    }
}

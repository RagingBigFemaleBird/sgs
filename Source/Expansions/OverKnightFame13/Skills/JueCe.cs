using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using System.Diagnostics;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.OverKnightFame13.Skills
{
    public class JueCe : TriggerSkill
    {
        public JueCe()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Cards.Any(c => c[Card.IsLastHandCard] == 1) && Game.CurrentGame.CurrentPlayer == p; },
                (p, e, a) =>
                {
                    var card = a.Cards.FirstOrDefault(c => c[Card.IsLastHandCard] == 1);
                    if (card != null && card.HistoryPlace1.Player != null)
                        Game.CurrentGame.DoDamage(p, card.HistoryPlace1.Player, 1, DamageElement.None, null, null);
                },
                TriggerCondition.Global
            ) {  };
            Triggers.Add(GameEvent.CardsLost, trigger);
            IsAutoInvoked = null;
        }
    }
}

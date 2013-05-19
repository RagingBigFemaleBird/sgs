using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.OverKnightFame13.Skills
{
    /// <summary>
    /// 夺刀-每当你受到一次伤害时,你可以弃之一张牌,获得伤害来源的武器牌
    /// </summary>
    public class DuoDao : TriggerSkill
    {
        class DuoDaoVerifier : CardsAndTargetsVerifier
        {
            public DuoDaoVerifier()
            {
                MaxCards = 1;
                MinCards = 1;
                MaxPlayers = 0;
            }

            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Place.DeckType == DeckType.Hand;
            }
        }
        public DuoDao()
        {
            var trigger = new AutoNotifyUsagePassiveSkillTrigger(
                this,
                (p, e, a) => { return p.HandCards().Count > 0 && a.Source != null && a.Source.Weapon() != null; },
                (p, e, a, cards, players) =>
                {
                    Game.CurrentGame.HandleCardDiscard(p, cards);
                    Game.CurrentGame.HandleCardTransferToHand(a.Source, p, new List<Card>() { a.Source.Weapon() });
                },
                TriggerCondition.OwnerIsTarget,
                new DuoDaoVerifier()
            ) { AskForConfirmation = false };
            Triggers.Add(GameEvent.AfterDamageInflicted, trigger);

            IsAutoInvoked = null;
        }
    }
}

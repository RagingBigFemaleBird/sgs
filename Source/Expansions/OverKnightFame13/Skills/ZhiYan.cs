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
    public class ZhiYan : TriggerSkill
    {
        class ZhiYanVerifier : CardsAndTargetsVerifier
        {
            public ZhiYanVerifier()
            {
                MaxCards = 0;
                MinCards = 0;
                MaxPlayers = 1;
                MinPlayers = 1;
            }
            protected override bool VerifyPlayer(Player source, Player player)
            {
                return true;
            }
        }

        public ZhiYan()
        {
            var trigger = new AutoNotifyUsagePassiveSkillTrigger(
                this,
                (p, e, a) => { return true; },
                (p, e, a, c, pls) =>
                {
                    Game.CurrentGame.SyncImmutableCardAll(Game.CurrentGame.PeekCard(0));
                    Card card = Game.CurrentGame.PeekCard(0);
                    Game.CurrentGame.DrawCards(pls[0], 1);
                    if (card.Type.BaseCategory() == CardCategory.Equipment)
                    {
                        Game.CurrentGame.RecoverHealth(pls[0], pls[0], 1);
                        var args = new GameEventArgs();
                        args.Source = pls[0];
                        args.Targets = new List<Player>();
                        args.Skill = null;
                        args.Cards = new List<Card>() { card };
                        Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
                    }
                },
                TriggerCondition.OwnerIsSource,
                new ZhiYanVerifier()
            ) { };

            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger);
        }
    }
}

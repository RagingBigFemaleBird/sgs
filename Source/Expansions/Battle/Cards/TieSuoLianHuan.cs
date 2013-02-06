using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;

namespace Sanguosha.Expansions.Battle.Cards
{
    
    public class TieSuoLianHuan : CardHandler
    {
        // WARNING: MASSIVE UGLY HACK for 蛊惑 and similar skills
        public static readonly CardAttribute ProhibitReforging = CardAttribute.Register("ProhibitReforging");
        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard cardr, GameEventArgs inResponseTo)
        {
            dest.IsIronShackled = !dest.IsIronShackled;
        }

        public override void Process(GameEventArgs handlerArgs)
        {
            var source = handlerArgs.Source;
            var dests = handlerArgs.Targets;
            if (dests == null || dests.Count == 0)
            {
                Game.CurrentGame.DrawCards(source, 1);
            }
            else
            {
                base.Process(handlerArgs);
            }
        }
        public override bool IsReforging(Player source, ISkill skill, List<Card> cards, List<Player> targets)
        {
            if (targets == null || targets.Count == 0)
            {
                return true;
            }
            return false;
        }

        public override void TagAndNotify(Player source, List<Player> dests, ICard card, GameAction action = GameAction.Use)
        {
            if (this.IsReforging(source, null, null, dests))
            {
                if (card is CompositeCard)
                {
                    foreach (Card c in (card as CompositeCard).Subcards)
                    {
                        c.Log.Source = source;
                        c.Log.GameAction = GameAction.Reforge;
                    }

                }
                else
                {
                    var c = card as Card;
                    Trace.Assert(card != null);
                    c.Log.Source = source;
                    c.Log.GameAction = GameAction.Reforge;
                }
                Game.CurrentGame.NotificationProxy.NotifyReforge(source, card);
                return;
            }
            base.TagAndNotify(source, dests, card, action);
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            if (targets != null && targets.Count >= 3)
            {
                return VerifierResult.Fail;
            }
            if (targets == null || targets.Count == 0 && card[ProhibitReforging] == 1)
            {
                return VerifierResult.Partial;
            }
            return VerifierResult.Success;
        }

        public override CardCategory Category
        {
            get { return CardCategory.ImmediateTool; }
        }
    }

}

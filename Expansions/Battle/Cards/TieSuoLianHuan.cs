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
        protected override void Process(Player source, Player dest, ICard card)
        {
            throw new NotImplementedException();
        }

        public override void Process(Player source, List<Player> dests, ICard card)
        {
            if (dests == null || dests.Count == 0)
            {
                Game.CurrentGame.DrawCards(source, 1);
            }
            else
            {
                Game.CurrentGame.PlayerUsedCard(source, card);
                foreach (var player in dests)
                {
                    if (PlayerIsCardTargetCheck(source, player))
                    {
                        player[PlayerAttribute.IronShackled] = 1 - player[PlayerAttribute.IronShackled];
                    }
                }
            }
        }

        public override VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> targets)
        {
            if (targets == null || targets.Count == 0)
            {
                return VerifyHelper(source, skill, cards, targets, false);
            }
            else
            {
                return VerifyHelper(source, skill, cards, targets, true);
            }
        }
        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            if (targets != null && targets.Count >= 3)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public override CardCategory Category
        {
            get { return CardCategory.ImmediateTool; }
        }
    }
}

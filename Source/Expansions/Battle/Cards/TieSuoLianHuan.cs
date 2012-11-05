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
                foreach (var player in dests)
                {
                    Player p = player;
                    Player src = source;
                    if (PlayerIsCardTargetCheck(ref src, ref p, card))
                    {
                        p[Player.IronShackled] = 1 - p[Player.IronShackled];
                    }
                }
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

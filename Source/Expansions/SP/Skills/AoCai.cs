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

namespace Sanguosha.Expansions.SP.Skills
{
    public class AoCai : CardTransformSkill
    {
        public override VerifierResult TryTransform(List<Card> cards, List<Player> arg, out CompositeCard card, bool isPlay)
        {
            card = new CompositeCard();
            card.Subcards = new List<Card>();
            card.Type = null;
            if (cards != null && cards.Count > 0)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public AoCai()
        {
        }

        public CardHandler AdditionalType { get; set; }
    }
}

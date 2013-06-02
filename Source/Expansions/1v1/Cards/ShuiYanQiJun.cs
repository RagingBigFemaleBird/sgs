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

namespace Sanguosha.Expansions._1v1.Cards
{
    public class ShuiYanQiJun : CardHandler
    {
        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
        {
            IPlayerProxy ui = Game.CurrentGame.UiProxies[source];
            if (source.IsDead) return;
            if (dest.Equipments().Count == 0)
            {
                Game.CurrentGame.DoDamage(source, dest, 1, DamageElement.None, null, null);
                return;
            }
            int answer;
            if (ui.AskForMultipleChoice(new MultipleChoicePrompt("ShuiYanQiJun"), OptionPrompt.YesNoChoices, out answer) && answer == 1)
            {
                Game.CurrentGame.HandleCardDiscard(dest, new List<Card>(dest.Equipments()));
            }
            else
            {
                Game.CurrentGame.DoDamage(source, dest, 1, DamageElement.None, null, null);
            }
        }

        public override VerifierResult Verify(Player source, ICard card, List<Player> targets, bool isLooseVerify)
        {
            if (targets == null || targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (!isLooseVerify && targets.Count > 1)
            {
                return VerifierResult.Fail;
            }

            foreach (var player in targets)
            {
                if (player == source)
                {
                    return VerifierResult.Fail;
                }
            }
            return VerifierResult.Success;
        }

        public override CardCategory Category
        {
            get { return CardCategory.ImmediateTool; }
        }
    }
}

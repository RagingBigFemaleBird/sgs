using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;

namespace Sanguosha.Core.UI
{
    public interface IGlobalUiProxy
    {
        bool AskForCardUsage(Prompt prompt, ICardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players, out Player respondingPlayer);
        void AskForMultipleCardUsage(Prompt prompt, ICardUsageVerifier verifier, List<Player> players, out Dictionary<Player, ISkill> askill, out Dictionary<Player, List<Card>> acards, out Dictionary<Player, List<Player>> aplayers);
        void AskForHeroChoice(Dictionary<Player, List<Card>> restDraw, Dictionary<Player, List<Card>> heroSelection, int numberOfHeroes, ICardChoiceVerifier verifier);
        void AskForMultipleChoice(Prompt prompt, List<OptionPrompt> questions, List<Player> players, out Dictionary<Player, int> answer);
        void Abort();
    }
}

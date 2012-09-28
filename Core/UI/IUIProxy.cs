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
    public enum VerifierResult
    {
        Success,
        Partial,
        Fail,
        Retry,
    }
    
    public interface ICardUsageVerifier
    {
        VerifierResult Verify(ISkill skill, List<Card> cards, List<Player> players);
    }
    public interface ICardChoiceVerifier
    {
        VerifierResult Verify(List<List<Card>> answer);
    }

    public interface IUiProxy
    {
        Player HostPlayer { get; set; }
        bool AskForCardUsage(string prompt, ICardUsageVerifier verifier,
                             out ISkill skill, out List<Card> cards, out List<Player> players);
        bool AskForCardChoice(List<DeckPlace> sourceDecks, List<string> resultDeckNames,
                              List<int> resultDeckMaximums,
                              ICardChoiceVerifier verifier, out List<List<Card>> answer);
    }

}

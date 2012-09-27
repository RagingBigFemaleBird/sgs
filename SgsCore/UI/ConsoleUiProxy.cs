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
    public class ConsoleUiProxy : IUiProxy
    {
        private Player hostPlayer;

        public Player HostPlayer
        {
            get { return hostPlayer; }
            set { hostPlayer = value; }
        }

        void IUiProxy.AskForCardUsage(string prompt, ICardUsageVerifier verifier, out ISkill skill, out List<Card> cards, out List<Player> players)
        {
            Player p = hostPlayer;
            cards = new List<Card>();
            VerifierResult r = verifier.Verify(null, null, null);
            skill = null;
            if (r == VerifierResult.Partial)
            {
                goto justPlayers;
            }
            Console.Write("Ask for card usage: ");
            int i = 0;
            foreach (ICard card in Game.CurrentGame.Decks[p, DeckType.Hand])
            {
                Console.Write(" Card {0} {1}{2}{3}, ", i, card.Suit, card.Rank, card.Type);
                i++;
            }
        again:
            Console.Write("Card id, -1 to trigger");
            string ids = Console.ReadLine();
            int id = int.Parse(ids);
            if (id >= Game.CurrentGame.Decks[p, DeckType.Hand].Count)
            {
                Console.WriteLine("Out of range");
                goto again;
            }
            if (id < -1)
            {
                cards = null;
                players = null;
            }
            else
            {
                if (id == -1)
                {
                    Console.Write("Skill ID:");
                    ids = Console.ReadLine();
                    id = int.Parse(ids);
                    // find in game that skill NOW!!!!
                    // but we only want 火鸡 at this time
                    skill = new Skills.Fire.HuoJi();
                    goto again;
                }
                cards.Add(Game.CurrentGame.Decks[p, DeckType.Hand][id]);
                players = null;
                r = verifier.Verify(skill, cards, players);
                if (r == VerifierResult.Success)
                {
                    return;
                }
                if (r == VerifierResult.Fail)
                {
                    Console.Write("Failed check, again? 1 yes 0 no");
                    ids = Console.ReadLine();
                    id = int.Parse(ids);
                    if (id == 1)
                    {
                        goto again;
                    }
                }
            }
        justPlayers:
            {
                players = new List<Player>();
                while (true)
                {
                    Console.WriteLine("");
                    Console.Write("Target player: -1 to end");
                    ids = Console.ReadLine();
                    id = int.Parse(ids);
                    if (id < 0)
                    {
                        break;
                    }
                    if (id > Game.CurrentGame.Players.Count)
                    {
                        Console.WriteLine("out of range");
                    }
                    else
                    {
                        players.Add(Game.CurrentGame.Players[id]);
                    }
                    r = verifier.Verify(skill, cards, players);
                    if (r == VerifierResult.Partial)
                    {
                        Console.WriteLine("Require more");
                    }
                    if (r == VerifierResult.Fail)
                    {
                        Console.WriteLine("Failed check");
                        players.Remove(Game.CurrentGame.Players[id]);
                    }
                }
                r = verifier.Verify(skill, cards, players);
                if (r == VerifierResult.Success)
                {
                    return;
                }
                if (r == VerifierResult.Fail)
                {
                    Console.Write("Failed check, again? 1 yes 0 no");
                    ids = Console.ReadLine();
                    id = int.Parse(ids);
                    if (id == 1)
                    {
                        goto again;
                    }
                }
                players = null;
                skill = null;
                cards = null;
                return;

            }
        }

        void IUiProxy.AskForCardChoice(List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, out List<List<Card>> answer)
        {
            answer = null;
        }

    }
}

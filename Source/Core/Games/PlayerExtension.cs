using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;


namespace Sanguosha.Core.Games
{
    public static class PlayerExtension
    {
        public static List<Card> HandCards(this Player p)
        {
            return Game.CurrentGame.Decks[p, DeckType.Hand];
        }

        public static List<Card> Equipments(this Player p)
        {
            return Game.CurrentGame.Decks[p, DeckType.Equipment];
        }

        public static List<Card> DelayedTools(this Player p)
        {
            return Game.CurrentGame.Decks[p, DeckType.DelayedTools];
        }

        public static Card Weapon(this Player p)
        {
            return Game.CurrentGame.Decks[p, DeckType.Equipment].FirstOrDefault(c => c.Type is Weapon);
        }

        public static Card Armor(this Player p)
        {
            return Game.CurrentGame.Decks[p, DeckType.Equipment].FirstOrDefault(c => c.Type is Armor);
        }

        public static Card DefensiveHorse(this Player p)
        {
            return Game.CurrentGame.Decks[p, DeckType.Equipment].FirstOrDefault(c => c.Type is DefensiveHorse);
        }

        public static Card OffensiveHorse(this Player p)
        {
            return Game.CurrentGame.Decks[p, DeckType.Equipment].FirstOrDefault(c => c.Type is OffensiveHorse);
        }

        /// <summary>
        /// 询问使用或打出卡牌，可以发动技能。
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="verifier"></param>
        /// <param name="skill"></param>
        /// <param name="cards"></param>
        /// <param name="players"></param>
        /// <returns>False if user cannot provide an answer.</returns>
        public static bool AskForCardUsage(this Player p, Prompt prompt, ICardUsageVerifier verifier,
                                           out ISkill skill, out List<Card> cards, out List<Player> players)
        {
            return Game.CurrentGame.UiProxies[p].AskForCardUsage(prompt, verifier, out skill, out cards, out players);
        }

        /// <summary>
        /// 询问用户从若干牌堆中选择卡牌，例如顺手牵羊，观星等等。
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="sourceDecks"></param>
        /// <param name="resultDeckNames"></param>
        /// <param name="resultDeckMaximums"></param>
        /// <param name="verifier"></param>
        /// <param name="answer">用户选择结果。对应resultDeckNames，每个选出的牌堆占用一个list。</param>
        /// <returns>False if user cannot provide an answer.</returns>
        public static bool AskForCardChoice(this Player p, Prompt prompt,
                                            List<DeckPlace> sourceDecks,
                                            List<string> resultDeckNames,
                                            List<int> resultDeckMaximums,
                                            ICardChoiceVerifier verifier,
                                            out List<List<Card>> answer,
                                            AdditionalCardChoiceOptions helper = null,
                                            CardChoiceRearrangeCallback callback = null)
        {
            return Game.CurrentGame.UiProxies[p].AskForCardChoice(prompt, sourceDecks, resultDeckNames, resultDeckMaximums, verifier, out answer, helper, callback);
        }

        /// <summary>
        /// 询问多选题目，例如是否发动洛神
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="questions">问题列表</param>
        /// <param name="answer">回答</param>
        /// <returns></returns>
        public static bool AskForMultipleChoice(this Player p, Prompt prompt, List<OptionPrompt> options, out int answer)
        {
            return Game.CurrentGame.UiProxies[p].AskForMultipleChoice(prompt, options, out answer);
        }
    }
}

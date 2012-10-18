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
    }

    public class UICardMovement
    {
        public Card Card { get; set; }
        public DeckPlace Source { get; set; }
        public DeckPlace To { get; set; }
        public IGameLog Note { get; set; }
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
        /// <summary>
        /// 询问使用或打出卡牌，可以发动技能。
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="verifier"></param>
        /// <param name="skill"></param>
        /// <param name="cards"></param>
        /// <param name="players"></param>
        /// <returns>False if user cannot provide an answer.</returns>
        bool AskForCardUsage(string prompt, ICardUsageVerifier verifier,
                             out ISkill skill, out List<Card> cards, out List<Player> players);
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
        bool AskForCardChoice(string prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames,
                              List<int> resultDeckMaximums,
                              ICardChoiceVerifier verifier, out List<List<Card>> answer);

        /// <summary>
        /// 询问多选题目，例如是否发动大姨妈
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="questions">问题列表</param>
        /// <param name="answer">回答</param>
        /// <returns></returns>
        bool AskForMultipleChoice(string prompt, List<string> questions, out int answer);

        void NotifyUiLog(List<UICardMovement> moves);
    }
}

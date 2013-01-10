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
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.StarSP.Skills
{
    /// <summary>
    /// 雪恨―锁定技，一名角色的回合结束阶段开始时，若你为无节操状态，你须捡回节操并选择一项：1、弃置其等同于你已损失体力值数的牌；2、视为对一名角色使用一张【杀】。
    /// </summary>
    public class XueHen : TriggerSkill
    {
        protected override int GenerateSpecialEffectHintIndex(Player source, List<Player> targets)
        {
            return source[XueHenEffect];
        }

        class XueHenShaVerifier : CardsAndTargetsVerifier
        {
            public XueHenShaVerifier()
            {
                MaxCards = 0;
                MinCards = 0;
                MaxPlayers = 1;
                MinPlayers = 1;
                Discarding = false;
            }
        }

        class XueHenCardChoiceVerifier : ICardChoiceVerifier
        {
            public VerifierResult Verify(List<List<Card>> answer)
            {
                if (answer != null && answer[0].Count > count)
                {
                    return VerifierResult.Fail;
                }
                if (answer == null || answer[0] == null || answer[0].Count < count)
                {
                    return VerifierResult.Partial;
                }
                return VerifierResult.Success;
            }
            int count;
            public XueHenCardChoiceVerifier(int count)
            {
                this.count = count;
            }
            public UiHelper Helper
            {
                get { return null; }
            }
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            Owner[FenYong.FenYongStatus] = 0;
            int answer = 0;
            Player current = Game.CurrentGame.CurrentPlayer;
            int choiceCount = Owner.LostHealth;
            int currentPlayerCardsCount = current.HandCards().Count + current.Equipments().Count();
            List<OptionPrompt> prompts = new List<OptionPrompt>();
            prompts.Add(new OptionPrompt("XueHenQiPai", current, choiceCount));
            prompts.Add(new OptionPrompt("XueHenSha"));
            Owner.AskForMultipleChoice(new MultipleChoicePrompt("XueHen"), prompts, out answer);
            if (answer == 0)
            {
                Owner[XueHenEffect] = 0;
                NotifySkillUse();
                if (currentPlayerCardsCount <= choiceCount)
                {
                    List<Card> cards = new List<Card>();
                    cards.AddRange(current.HandCards());
                    cards.AddRange(current.Equipments());
                    Game.CurrentGame.HandleCardDiscard(current, cards);
                    return;
                }
                List<List<Card>> choiceAnswer;
                List<DeckPlace> sourcePlace = new List<DeckPlace>();
                sourcePlace.Add(new DeckPlace(current, DeckType.Hand));
                sourcePlace.Add(new DeckPlace(current, DeckType.Equipment));
                if (!Owner.AskForCardChoice(new CardChoicePrompt("XueHen", current, Owner),
                    sourcePlace,
                    new List<string>() { "QiPaiDui" },
                    new List<int>() { choiceCount },
                    new XueHenCardChoiceVerifier(choiceCount),
                    out choiceAnswer,
                    null,
                    CardChoiceCallback.GenericCardChoiceCallback))
                {
                    choiceAnswer = new List<List<Card>>();
                    choiceAnswer[0].AddRange(current.HandCards());
                    choiceAnswer[0].AddRange(current.Equipments());
                    choiceAnswer[0] = choiceAnswer[0].GetRange(0, choiceCount);
                }
                Game.CurrentGame.HandleCardDiscard(current, choiceAnswer[0]);
            }
            else
            {
                ISkill skill;
                List<Card> cards;
                List<Player> players;
                if (!Owner.AskForCardUsage(new CardUsagePrompt("XueHen"), new XueHenShaVerifier(), out skill, out cards, out players))
                {
                    players = new List<Player>();
                    List<Player> nPlayers = Game.CurrentGame.AlivePlayers;
                    players.Add(nPlayers[0]);
                }
                Owner[XueHenEffect] = 1;
                NotifySkillUse(players);
                GameEventArgs args = new GameEventArgs();
                Owner[Sha.NumberOfShaUsed]--;
                args.Source = Owner;
                args.Targets = players;
                args.Skill = new CardWrapper(Owner, new RegularSha());
                Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
            }
        }

        public XueHen()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Owner[FenYong.FenYongStatus] == 1; },
                Run,
                TriggerCondition.Global
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger);
            IsEnforced = true;
        }
        private static readonly PlayerAttribute XueHenEffect = PlayerAttribute.Register("XueHenEffect");
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Utils;
using System.Diagnostics;

namespace Sanguosha.Expansions.Pk1v1.Skills
{
    /// <summary>
    /// 谋诛―出牌阶段，你可以令对手交给你一张手牌。然后若你的手牌数大于对手的手牌数，对手选择一项：视为对你使用一张【杀】；视为对你使用一张【决斗】。每阶段限一次。
    /// </summary>
    public class MouZhu : ActiveSkill
    {
        public override VerifierResult Validate(GameEventArgs arg)
        {
            if (Owner[MouZhuUsed] != 0)
            {
                return VerifierResult.Fail;
            }
            List<Card> cards = arg.Cards;
            if (cards != null && cards.Count > 0)
            {
                return VerifierResult.Fail;
            }
            if (Owner.HandCards().Count == 0)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets != null && arg.Targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets != null && arg.Targets.Count == 1)
            {
                if (arg.Targets[0] == Owner) return VerifierResult.Fail;
                if (arg.Targets[0].HandCards().Count == 0) return VerifierResult.Fail;
            }

            if (arg.Targets == null || arg.Targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            return VerifierResult.Success;
        }

        public class MouZhuVerifier : CardsAndTargetsVerifier
        {
            public MouZhuVerifier()
            {
                MinCards = 1;
                MaxCards = 1;
                MinPlayers = 0;
                MaxPlayers = 0;
            }
            protected override bool VerifyCard(Player source, Card card)
            {
                return card.Place.DeckType == DeckType.Hand;
            }
            protected override bool VerifyPlayer(Player source, Player player)
            {
                return true;
            }
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[MouZhuUsed] = 1;
            List<Card> cards;
            List<Player> players;
            ISkill skill;
            if (!arg.Targets[0].AskForCardUsage(new CardUsagePrompt("MouZhu", Owner), new MouZhuVerifier(), out skill, out cards, out players))
            {
                cards = Game.CurrentGame.PickDefaultCardsFrom(new List<DeckPlace>() { new DeckPlace(arg.Targets[0], DeckType.Hand) });
            }
            Game.CurrentGame.HandleCardTransferToHand(arg.Targets[0], Owner, cards);
            if (Owner.HandCards().Count > arg.Targets[0].HandCards().Count)
            {
                int answer;
                arg.Targets[0].AskForMultipleChoice(new MultipleChoicePrompt("MouZhu"), new List<OptionPrompt>() { new OptionPrompt("MouZhuSha", Owner), new OptionPrompt("MouZhuJueDou", Owner) }, out answer);
                if (answer == 0)
                {
                    Sha.UseDummyShaTo(arg.Targets[0], Owner, new RegularSha(), new CardUsagePrompt("MouZhu2", Owner));
                }
                else
                {
                    GameEventArgs args = new GameEventArgs();
                    args.Source = arg.Targets[0];
                    args.Targets = new List<Player>();
                    args.Targets.Add(Owner);
                    args.Skill = new CardWrapper(arg.Targets[0], new JueDou());
                    args.Cards = new List<Card>();
                    Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
                }
            }
            return true;
        }

        public static PlayerAttribute MouZhuUsed = PlayerAttribute.Register("MouZhuUsed", true);

    }
}

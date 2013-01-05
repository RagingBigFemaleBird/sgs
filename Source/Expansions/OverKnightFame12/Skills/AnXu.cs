using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    /// <summary>
    /// 安恤-出牌阶段，你可以指定其他两名手牌数不同的角色，其中手牌少的角色抽取手牌较多的角色一张手牌并展示之，如果该牌非黑桃，则你摸一张牌。每阶段限一次。
    /// </summary>
    public class AnXu : ActiveSkill
    {
        public override VerifierResult Validate(GameEventArgs arg)
        {
            if (Owner[AnXuUsed] != 0)
            {
                return VerifierResult.Fail;
            }
            List<Card> cards = arg.Cards;
            if ((cards == null || cards.Count == 0) && (arg.Targets == null || arg.Targets.Count == 0))
            {
                return VerifierResult.Partial;
            }
            if (cards != null && cards.Count > 0)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets != null && arg.Targets.Count > 2)
            {
                return VerifierResult.Fail;
            }
            foreach (Player p in arg.Targets)
            {
                if (p == Owner)
                {
                    return VerifierResult.Fail;
                }
            }
            if (arg.Targets != null && arg.Targets.Count == 2)
            {
                if(Game.CurrentGame.Decks[arg.Targets[0], DeckType.Hand].Count == Game.CurrentGame.Decks[arg.Targets[1], DeckType.Hand].Count)
					return VerifierResult.Fail;
            }
            if (arg.Targets == null || arg.Targets.Count < 2)
            {
                return VerifierResult.Partial;
            }
            return VerifierResult.Success;
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[AnXuUsed] = 1;
			Player less=arg.Targets[0];
			Player more=arg.Targets[1];
			if(Game.CurrentGame.Decks[arg.Targets[0], DeckType.Hand].Count>Game.CurrentGame.Decks[arg.Targets[1], DeckType.Hand].Count)
			{
				less=arg.Targets[1];
				more=arg.Targets[0];
			}
			List<List<Card>> answer;
			if (!Game.CurrentGame.UiProxies[less].AskForCardChoice(new CardChoicePrompt("AnXu"), new List<DeckPlace>() { new DeckPlace(more, DeckType.Hand) },
			new List<string>() { "AnXu" }, new List<int>() { 1 }, new RequireOneCardChoiceVerifier(), out answer))
			{
				answer = new List<List<Card>>();
				answer.Add(new List<Card>());
				answer[0].Add(Game.CurrentGame.Decks[more, DeckType.Hand][0]);
			}
			Game.CurrentGame.HandleCardTransferToHand(more, less, answer[0]);
            var theCard = answer[0][0];
            Game.CurrentGame.SyncCardAll(ref theCard);
			Game.CurrentGame.NotificationProxy.NotifyShowCard(less, theCard);
			if (theCard.Suit!= SuitType.Spade)
				Game.CurrentGame.DrawCards(Owner,1);
			return true;
        }
        public static PlayerAttribute AnXuUsed = PlayerAttribute.Register("AnXuUsed", true);

    }
}

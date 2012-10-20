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

namespace Sanguosha.Expansions.Basic.Skills
{
    /// <summary>
    /// 反间-出牌阶段，你可以指定一名其他角色，该角色选择一种花色后获得你的一张手牌并展示之，若此牌与所选花色不同，则你对该角色造成1点伤害。每阶段限一次。
    /// </summary>
    public class FanJian : ActiveSkill
    {
        public override VerifierResult Validate(GameEventArgs arg)
        {
            if (Owner[FanJianUsed] != 0)
            {
                return VerifierResult.Fail;
            }
            if (Game.CurrentGame.Decks[Owner, DeckType.Hand].Count == 0)
            {
                return VerifierResult.Fail;
            }
            if (arg.Cards != null || arg.Cards.Count != 0)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets != null && arg.Targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets == null || arg.Targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (arg.Targets != null && arg.Targets[0] == Owner)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public class FanJianVerifier : ICardChoiceVerifier
        {
            public VerifierResult Verify(List<List<Card>> answer)
            {
                Trace.Assert(answer.Count == 1);
                if (answer[0].Count == 0)
                {
                    return VerifierResult.Partial;
                }
                return VerifierResult.Success;
            }
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[FanJianUsed] = 1;
            // suit guess
            SuitType suit = SuitType.Diamond;
            List<DeckPlace> decks = new List<DeckPlace>();
            decks.Add(new DeckPlace(Owner, DeckType.Hand));
            List<int> max = new List<int>();
            max.Add(1);
            List<string> decknames = new List<string>();
            decknames.Add("FanJianChoice");
            List<List<Card>> answer;
            FanJianVerifier ver = new FanJianVerifier();
            Card theCard;
            if (!Game.CurrentGame.UiProxies[arg.Targets[0]].AskForCardChoice("FanJian", decks, decknames, max, ver, out answer))
            {
                Trace.TraceInformation("Invalid answer from user");
                theCard = Game.CurrentGame.Decks[Owner, DeckType.Hand][0];
            }
            else
            {
                theCard = answer[0][0];
            }
            List<Card> clist = new List<Card>();
            clist.Add(theCard);
            if (theCard.Suit != suit)
            {
                Trace.TraceInformation("Guessed wrong");
                Game.CurrentGame.DoDamage(Owner, arg.Targets[0], 1, DamageElement.None, null);
            }
            CardsMovement m = new CardsMovement();
            m.cards = clist;
            m.to = new DeckPlace(arg.Targets[0], DeckType.Hand);
            Game.CurrentGame.MoveCards(m, null);
            return true;
        }

        public static string FanJianUsed = "FanJianUsed";
        public override Core.Players.Player Owner
        {
            get
            {
                return base.Owner;
            }
            set
            {
                base.Owner = value;
                Owner.AutoResetAttributes.Add(FanJianUsed);
            }
        }
    }
}

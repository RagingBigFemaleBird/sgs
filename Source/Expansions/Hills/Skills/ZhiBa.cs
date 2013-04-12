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
using Sanguosha.Expansions.Hills.Skills;

namespace Sanguosha.Expansions.Hills.Skills
{
    /// <summary>
    /// 制霸―主公技，其他吴势力角色的出牌阶段，可与你拼点，若该角色没赢，你可以获得双方拼点的牌，每阶段限一次；"魂姿"发动后，你可以拒绝此拼点。
    /// </summary>
    public class ZhiBaGivenSkill : ActiveSkill, IRulerGivenSkill
    {
        private static readonly PlayerAttribute ZhiBaUsed = PlayerAttribute.Register("ZhiBaUsedUsed", true);
        public override VerifierResult Validate(GameEventArgs arg)
        {            
            if (Owner[ZhiBaUsed[Master]] != 0)
            {
                return VerifierResult.Fail;
            }
            if (arg.Targets != null && arg.Targets.Count > 0)
            {
                return VerifierResult.Fail;
            }
            List<Card> cards = arg.Cards;
            if (cards != null && cards.Count > 0)
            {
                return VerifierResult.Fail;
            }
            if (Game.CurrentGame.Decks[Master, DeckType.Hand].Count == 0 || Owner.HandCards().Count == 0)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public override bool Commit(GameEventArgs arg)
        {
            Owner[ZhiBaUsed[Master]] = 1;
            if (Master[HunZi.HunZiAwakened] == 1)
            {
                int answer;
                if (Game.CurrentGame.UiProxies[Master].AskForMultipleChoice(new MultipleChoicePrompt("ZhiBa", Owner), Prompt.YesNoChoices, out answer) && answer == 0)
                {
                    return true;
                }
            }
            Card card1, card2;
            bool c1, c2;
            var ret = Game.CurrentGame.PinDianReturnCards(Owner, Master, out card1, out card2, this, out c1, out c2);
            if (ret != true)
            {
                int answer;
                if (Game.CurrentGame.UiProxies[Master].AskForMultipleChoice(new MultipleChoicePrompt("ZhiBaHuoDe"), Prompt.YesNoChoices, out answer) && answer == 1)
                {
                    Game.CurrentGame.EnterAtomicContext();
                    Game.CurrentGame.PlayerLostCard(Owner, new List<Card>() { card1 });
                    Game.CurrentGame.PlayerLostCard(Master, new List<Card>() { card2 });
                    var cardList = new List<Card>();
                    if (!c1) cardList.Add(card1);
                    if (!c2) cardList.Add(card2);
                    Game.CurrentGame.HandleCardTransferToHand(null, Master, cardList);
                    Game.CurrentGame.ExitAtomicContext();
                    return true;
                }
            }
            Game.CurrentGame.EnterAtomicContext();
            if (!c1) Game.CurrentGame.PlaceIntoDiscard(Owner, new List<Card>() { card1 });
            if (!c2) Game.CurrentGame.PlaceIntoDiscard(Master, new List<Card>() { card2 });
            Game.CurrentGame.ExitAtomicContext();
            return true;
        }

        public ZhiBaGivenSkill()
        {
            Helper.HasNoConfirmation = true;
        }

        public Player Master { get; set; }
    }

    public class ZhiBa : RulerGivenSkillContainerSkill
    {
        public ZhiBa()
            : base(new ZhiBaGivenSkill(), Allegiance.Wu)
        {
        }
    }
}

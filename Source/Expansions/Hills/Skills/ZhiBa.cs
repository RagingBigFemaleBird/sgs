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
        public override VerifierResult Validate(GameEventArgs arg)
        {
            PlayerAttribute ZhiBaUsed = PlayerAttribute.Register("ZhiBaUsedUsed" + Master.Id, true);
            if (Owner[ZhiBaUsed] != 0)
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
            if (Game.CurrentGame.Decks[Master, DeckType.Hand].Count == 0)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public override bool Commit(GameEventArgs arg)
        {
            PlayerAttribute ZhiBaUsed = PlayerAttribute.Register("ZhiBaUsedUsed" + Master.Id, true);
            Owner[ZhiBaUsed] = 1;
            if (Master[HunZi.HunZiAwakened] == 1)
            {
                int answer;
                if (Game.CurrentGame.UiProxies[Master].AskForMultipleChoice(new MultipleChoicePrompt("ZhiBa"), Prompt.YesNoChoices, out answer) && answer == 0)
                {
                    return true;
                }
            }
            Card card1, card2;
            Game.CurrentGame.PinDianReturnCards(Owner, Master, out card1, out card2, this);
            if (card1.Rank <= card2.Rank)
            {
                int answer;
                if (Game.CurrentGame.UiProxies[Master].AskForMultipleChoice(new MultipleChoicePrompt("ZhiBaHuoDe"), Prompt.YesNoChoices, out answer) && answer == 1)
                {
                    Game.CurrentGame.EnterAtomicContext();
                    Game.CurrentGame.PlayerLostCard(Owner, new List<Card>() { card1 });
                    Game.CurrentGame.PlayerLostCard(Master, new List<Card>() { card2 });
                    Game.CurrentGame.HandleCardTransferToHand(null, Master, new List<Card>() { card1, card2 });
                    Game.CurrentGame.ExitAtomicContext();
                    return true;
                }
            }
            Game.CurrentGame.EnterAtomicContext();
            Game.CurrentGame.PlaceIntoDiscard(Owner, new List<Card>() { card1 });
            Game.CurrentGame.PlaceIntoDiscard(Master, new List<Card>() { card2 });
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

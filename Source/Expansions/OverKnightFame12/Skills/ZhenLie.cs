using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Expansions.Basic.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    /// <summary>
    /// 贞烈-在你的判定牌生效前，你可以从牌堆顶亮出一张牌代替之。
    /// </summary>
    public class ZhenLie : TriggerSkill
    {
        void OnJudgeBegin(Player player, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            Card c = Game.CurrentGame.Decks[eventArgs.Source, DeckType.JudgeResult].Last();
            int answer = 0;
            player.AskForMultipleChoice(new MultipleChoicePrompt("ZhenLie", c.Suit, c.Rank), OptionPrompt.YesNoChoices, out answer);
            if (answer == 1)
            {
                NotifySkillUse(new List<Player>());
                Game.CurrentGame.SyncImmutableCardAll(Game.CurrentGame.PeekCard(0));
                Card c1 = Game.CurrentGame.DrawCard();
                new GuiCai().ReplaceJudgementCard(null, eventArgs.Source, c1);
            }
        }

        public ZhenLie()
        {
            Triggers.Add(GameEvent.PlayerJudgeBegin, new RelayTrigger(OnJudgeBegin, TriggerCondition.OwnerIsSource));
            IsAutoInvoked = null;
        }
    }
}

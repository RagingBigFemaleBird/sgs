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

namespace Sanguosha.Expansions.OverKnightFame11.Skills
{
    /// <summary>
    /// 举荐-回合结束阶段开始时，你可以弃置一张非基本牌，令一名其他角色选择一项：摸两张牌，或回复1点体力，或将其武将牌翻至正面朝上并重置之。
    /// </summary>
    public class JuJian : TriggerSkill
    {
        class JuJianVerifier : CardsAndTargetsVerifier
        {
            public JuJianVerifier()
            {
                MaxCards = 1;
                MinCards = 1;
                MaxPlayers = 1;
                MinPlayers = 1;
                Discarding = true;
            }

            protected override bool VerifyCard(Player source, Card card)
            {
                return !card.Type.IsCardCategory(CardCategory.Basic);
            }

            protected override bool VerifyPlayer(Player source, Player player)
            {
                return player != source;
            }
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs, List<Card> cards, List<Player> players)
        {
            Game.CurrentGame.HandleCardDiscard(Owner, cards);
            Player target = players[0];
            List<OptionPrompt> options = new List<OptionPrompt>();
            options.Add(new OptionPrompt("JuJianMoPai"));
            if (target.LostHealth > 0)
            {
                options.Add(new OptionPrompt("JuJianHuiXue"));
            }
            if (target.IsImprisoned || target.IsIronShackled)
            {
                options.Add(new OptionPrompt("JuJianChongZhi"));
            }
            int answer = 0;
            if (options.Count > 1)
            {
                target.AskForMultipleChoice(new MultipleChoicePrompt("JuJian"), options, out answer);
            }
            if (answer == 0)
            {
                Game.CurrentGame.DrawCards(target, 2);
                return;
            }
            else if (answer == 1)
            {
                if (target.LostHealth > 0)
                {
                    Game.CurrentGame.RecoverHealth(Owner, target, 1);
                    return;
                }
            }
            else
            {
                target.IsImprisoned = false;
                target.IsIronShackled = false;
            }
        }

        public JuJian()
        {
            var trigger = new AutoNotifyUsagePassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsSource,
                new JuJianVerifier()
            );
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger);
            IsAutoInvoked = null;
        }
    }
}

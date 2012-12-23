using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 英魂-回合开始阶段开始时，若你已受伤，可令一名其他角色选择一项：摸X张牌，然后弃一张牌；或摸一张牌，然后弃X张牌(X为你已损失的体力值)。
    /// </summary>
    public class YingHun : TriggerSkill
    {
        public class YingHunVerifier : CardsAndTargetsVerifier
        {
            protected override bool VerifyPlayer(Player source, Player player)
            {
                return source != player;
            }

            public YingHunVerifier()
            {
                MinCards = 0;
                MaxCards = 0;
                MinPlayers = 1;
                MaxPlayers = 1;
            }
        }

        protected void Run(Player owner, GameEvent gameEvent, GameEventArgs eventArgs, List<Card> cards, List<Player> players)
        {
            int x = owner.MaxHealth - owner.Health;
            if (x == 1)
            {
                Game.CurrentGame.DrawCards(players[0], 1);
                Game.CurrentGame.ForcePlayerDiscard(players[0],
                                (p, i) =>
                                {
                                    return 1 - i;
                                },
                                true);
                return;
            };
            int answer = 0;
            Game.CurrentGame.UiProxies[owner].AskForMultipleChoice(new MultipleChoicePrompt("YingHun"), new List<string>() { Prompt.MultipleChoiceOptionPrefix + "YingHun1", Prompt.MultipleChoiceOptionPrefix + "YingHun2" }, out answer);
            if (answer == 0)
            {
                Game.CurrentGame.DrawCards(players[0], x);
                Game.CurrentGame.ForcePlayerDiscard(players[0],
                                (p, i) =>
                                {
                                    return 1 - i;
                                },
                                true);
            }
            else
            {
                Game.CurrentGame.DrawCards(players[0], 1);
                Game.CurrentGame.ForcePlayerDiscard(players[0],
                                (p, i) =>
                                {
                                    return x - i;
                                },
                                true);
            }
        }

        public YingHun()
        {
            var trigger = new AutoNotifyUsagePassiveSkillTrigger(
                this,
                (p, e, a) => { return p.MaxHealth > p.Health; },
                Run,
                TriggerCondition.OwnerIsSource,
                new YingHunVerifier()
            );
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Start], trigger);
            IsAutoInvoked = null;
        }
    }
}

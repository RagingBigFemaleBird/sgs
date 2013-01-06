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
using System.Diagnostics;

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 英魂-回合开始阶段开始时，若你已受伤，可令一名其他角色选择一项：摸X张牌，然后弃一张牌；或摸一张牌，然后弃X张牌(X为你已损失的体力值)。
    /// </summary>
    public class YingHun : TriggerSkill
    {
        protected override int GenerateSpecialEffectHintIndex(Player source, List<Player> targets)
        {
            Trace.Assert(Owner != null && Owner.Hero != null);
            if (Owner == null || Owner.Hero == null) return 0;
            else if (Owner.Hero.Name == "SunCe" || (Owner.Hero2 != null && Owner.Hero2.Name == "SunCe")) return 1;            
            return 0;
        }

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
            int x = owner.LostHealth;
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
            Game.CurrentGame.UiProxies[owner].AskForMultipleChoice(
                new MultipleChoicePrompt("YingHun", players[0]),
                new List<OptionPrompt>() { new OptionPrompt("YingHun1", x), new OptionPrompt("YingHun2", x) },
                out answer);
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
                (p, e, a) => { return p.LostHealth > 0; },
                Run,
                TriggerCondition.OwnerIsSource,
                new YingHunVerifier()
            );
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Start], trigger);
            IsAutoInvoked = null;
        }
    }
}

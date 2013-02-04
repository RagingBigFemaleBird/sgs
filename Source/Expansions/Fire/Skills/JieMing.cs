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
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Fire.Skills
{
    /// <summary>
    /// 节命-弃牌阶段，每当你受到一点伤害后，可令一名角色将手牌补至等同于其体力上限的张数（最多补至五张）。
    /// </summary>
    public class JieMing : TriggerSkill
    {
        class JieMingVerifier : CardsAndTargetsVerifier
        {
            public JieMingVerifier()
            {
                MaxPlayers = 1;
                MinPlayers = 1;
                MaxCards = 0;
                MinCards = 0;
            }

            protected override bool VerifyPlayer(Player source, Player player)
            {
                return true;
            }

            protected override bool VerifyCard(Player source, Card card)
            {
                return true;
            }
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            var args = eventArgs as DamageEventArgs;
            int damage = args.Magnitude;
            while (damage-- > 0)
            {
                if (Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("JieMing", this), new JieMingVerifier(), out skill, out cards, out players))
                {
                    NotifySkillUse(players);
                    Game.CurrentGame.DrawCards(players[0], Math.Min(5, players[0].MaxHealth - Game.CurrentGame.Decks[players[0], DeckType.Hand].Count));
                }
            }
        }


        public JieMing()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsTarget
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.AfterDamageInflicted, trigger);
            IsAutoInvoked = null;
        }
    }
}

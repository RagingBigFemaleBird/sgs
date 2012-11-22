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

namespace Sanguosha.Expansions.Wind.Skills
{
    /// <summary>
    /// 雷击-每当你使用或打出一张【闪】时，可令一名角色判定，若结果为黑桃，你对该角色造成2点雷电伤害。
    /// </summary>
    public class LeiJi : TriggerSkill
    {
        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("LeiJi"), new OneTargetNoSelfVerifier(),
                out skill, out cards, out players))
            {
                NotifySkillUse(players);
                var result = Game.CurrentGame.Judge(players[0]);
                if (result.Suit == SuitType.Spade)
                {
                    Game.CurrentGame.DoDamage(Owner, players[0], 2, DamageElement.Lightning, null, null);
                }
            }
        }

        public LeiJi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Card.Type is Shan; },
                Run,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Card.Type is Shan;},
                Run,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };

            Triggers.Add(GameEvent.PlayerPlayedCard, trigger);
            Triggers.Add(GameEvent.PlayerUsedCard, trigger2);
            IsAutoInvoked = null;
        }

    }
}

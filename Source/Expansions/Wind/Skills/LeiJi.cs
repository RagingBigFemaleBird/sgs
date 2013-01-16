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
        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs, List<Card> cards, List<Player> players)
        {
            var result = Game.CurrentGame.Judge(players[0], this, null, (judgeResultCard) => { return judgeResultCard.Suit == SuitType.Spade; });
            if (result.Suit == SuitType.Spade)
            {
                Game.CurrentGame.DoDamage(Owner, players[0], 2, DamageElement.Lightning, null, null);
            }
        }

        public class LeiJiVerifier : CardsAndTargetsVerifier
        {
            public LeiJiVerifier()
            {
                MinCards = 0;
                MaxCards = 0;
                MinPlayers = 1;
                MaxPlayers = 1;
            }
        }

        public LeiJi()
        {
            var trigger = new AutoNotifyUsagePassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard != null && a.ReadonlyCard.Type is Shan; },
                Run,
                TriggerCondition.OwnerIsSource,
                new LeiJiVerifier()
            );

            Triggers.Add(GameEvent.PlayerPlayedCard, trigger);
            Triggers.Add(GameEvent.PlayerUsedCard, trigger);
            IsAutoInvoked = null;
        }

    }
}

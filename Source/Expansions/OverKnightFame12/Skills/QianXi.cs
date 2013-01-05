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

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    public class QianXi : TriggerSkill
    {
        /// <summary>
        /// 潜袭-每当你使用【杀】对距离为1的目标角色造成伤害时，你可以进行一次判定，若判定结果不为红桃，你防止此伤害，改为令其减1点体力上限。
        /// </summary>
        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            var card = Game.CurrentGame.Judge(Owner, this, null, (judgeResultCard) => { return judgeResultCard.Suit != SuitType.Heart; });
            if (card.Suit != SuitType.Heart)
            {
                Game.CurrentGame.LoseMaxHealth(eventArgs.Targets[0], 1);
                throw new TriggerResultException(TriggerResult.End);
            }
        }
        public QianXi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Card.Type is Sha && Game.CurrentGame.DistanceTo(Owner, a.Targets[0]) == 1; },
                Run,
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.DamageCaused, trigger);
            IsAutoInvoked = false;
        }
    }
}

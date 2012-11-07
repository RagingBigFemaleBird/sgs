using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Fire.Skills
{
    /// <summary>
    /// 八阵-锁定技，若你的装备区没有防具牌，视为你装备着【八卦阵】。
    /// </summary>
    public class BaZhen : TriggerSkill
    {
        protected void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            NotifySkillUse(new List<Player>());
            ReadOnlyCard c = Game.CurrentGame.Judge(Owner, null, new Card() { Type = new BaGuaZhen() });
            if (c.SuitColor == SuitColorType.Red)
            {
                eventArgs.Cards = new List<Card>();
                ActionLog log = new ActionLog();
                log.Source = Owner;
                log.SkillAction = new Basic.Cards.BaGuaZhen.BaGuaZhenSkill();
                log.GameAction = GameAction.None;
                Game.CurrentGame.NotificationProxy.NotifySkillUse(log);
                throw new TriggerResultException(TriggerResult.Success);
            }
        }

        public BaZhen()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => 
                {
                    if (Game.CurrentGame.Decks[p, DeckType.Equipment].Any(card => card.Type is Armor))
                    {
                        return false;
                    }
                    return a.Card.Type is Shan;
                },
                Run,
                TriggerCondition.OwnerIsSource
            ) { IsAutoNotify = false, AskForConfirmation = true };
            Triggers.Clear();
            Triggers.Add(GameEvent.PlayerRequireCard, trigger);
        }

        public override bool IsEnforced
        {
            get
            {
                return true;
            }
        }
    }
}

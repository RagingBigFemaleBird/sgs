using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Sanguosha.Core.Triggers
{
    public class GetJudgeCardTrigger : Trigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (eventArgs.Source != Owner)
            {
                return;
            }
            if (!IsCorrectJudgeAction(eventArgs.Skill, eventArgs.Card)) return;
            //someone already took it...
            if (eventArgs.Cards.Count == 0)
            {
                return;
            }
            if (!doNotUnregister)
            {
                Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerJudgeDone, this);
            }
            var list = new List<Card>(eventArgs.Cards);
            eventArgs.Cards.Clear();
            GetJudgeCards(list);
            return;
        }

        protected virtual void GetJudgeCards(List<Card> list)
        {
            Game.CurrentGame.HandleCardTransferToHand(Owner, Owner, list);
        }

        protected virtual bool IsCorrectJudgeAction(ISkill skill, ICard card)
        {
            return skill == jSkill && card == jCard;
        }

        ISkill jSkill;
        ICard jCard;
        bool doNotUnregister = false;
        public GetJudgeCardTrigger(Player p, ISkill skill, ICard card, bool permenant = false)
        {
            Owner = p;
            jSkill = skill;
            jCard = card;
            doNotUnregister = permenant;
        }
    }
}

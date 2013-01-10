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
using Sanguosha.Expansions.Basic.Skills;

namespace Sanguosha.Expansions.StarSP.Skills
{
    /// <summary>
    /// 溃围–回合结束阶段开始时，你可以摸等同于场上所有玩家装备区内武器牌数+2的牌并将你的武将牌翻面。你的下个摸牌阶段开始时，你须弃置等同于场上所有玩家装备区内武器牌数的牌。
    /// </summary>
    public class KuiWei : TriggerSkill
    {
        void KuiWeiDisCards(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            Owner[KuiWeiStatus] = 0;
            int count = GetWeaponCount();
            if (count == 0 || Owner.HandCards().Count + Owner.Equipments().Count == 0) return;
            NotifySkillUse();
            if (count >= Owner.HandCards().Count + Owner.Equipments().Count)
            {
                List<Card> cards = new List<Card>();
                cards.AddRange(Owner.HandCards());
                cards.AddRange(Owner.Equipments());
                Game.CurrentGame.HandleCardDiscard(Owner, cards);
                return;
            }
            Game.CurrentGame.ForcePlayerDiscard(Owner, (p, d) => { return count - d; }, true);
        }

        int GetWeaponCount()
        {
            int count = 0;
            foreach (Player p in Game.CurrentGame.AlivePlayers)
            {
                if (p.Weapon() != null)
                    count++;
            }
            return count;
        }

        public KuiWei()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    int count = GetWeaponCount() + 2;
                    Game.CurrentGame.DrawCards(p, count);
                    p.IsImprisoned = !p.IsImprisoned;
                    p[KuiWeiStatus] = 1;
                },
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger);

            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                { return p[KuiWeiStatus] == 1; },
                KuiWeiDisCards,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Draw], trigger2);
        }

        private static readonly PlayerAttribute KuiWeiStatus = PlayerAttribute.Register("KuiWei", false, false, true);
    }
}

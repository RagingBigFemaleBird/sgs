using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Assassin.Skills
{
    /// <summary>
    /// 谋溃-当你使用【杀】指定一名角色为目标后，你可以选择一项：摸一张牌，或弃置其一张牌。若如此做，此【杀】被【闪】抵消时，该角色弃置你的一张牌。
    /// </summary>
    public class MouKui : TriggerSkill
    {
        protected override int GenerateSpecialEffectHintIndex(Player source, List<Player> targets)
        {
            return MouKuiEffect;
        }
        void Run1(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            MultipleChoicePrompt prompt;
            List<OptionPrompt> options = new List<OptionPrompt>();
            OptionPrompt option1 = new OptionPrompt("MouKuiMoPai");
            int i = 0;
            foreach (var target in eventArgs.Targets)
            {
                options.Clear();
                options.Add(OptionPrompt.NoChoice);
                options.Add(option1);
                options.Add(new OptionPrompt("MouKuiQiZhi", target));
                bool isNaked = target.HandCards().Count + target.Equipments().Count == 0;
                prompt = isNaked ? new MultipleChoicePrompt("MouKuiDrawCardOnly") : new MultipleChoicePrompt("MouKui");
                int answer = 0;
                Owner.AskForMultipleChoice(prompt, isNaked ? OptionPrompt.YesNoChoices : options, out answer);
                if (answer == 0) { i++; continue; }
                MouKuiEffect = 0;
                NotifySkillUse();
                if (answer == 1)
                {
                    Game.CurrentGame.DrawCards(Owner, 1);
                }
                else
                {
                    Card theCard = Game.CurrentGame.SelectACardFrom(target, Owner, new CardChoicePrompt("MouKui", target, Owner), "QiPaiDui");
                    Game.CurrentGame.HandleCardDiscard(target, new List<Card>() { theCard });
                }
                eventArgs.ReadonlyCard[MouKuiCheck[target]] |= (1 << i);
                i++;
            }
        }

        void Run2(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if ((eventArgs.ReadonlyCard[MouKuiCheck[eventArgs.Targets[0]]] & 1) == 1)
            {
                MouKuiEffect = 1;
                NotifySkillUse();
                Card theCard = Game.CurrentGame.SelectACardFrom(Owner, eventArgs.Targets[0], new CardChoicePrompt("MouKui", Owner, eventArgs.Targets[0]), "QiPaiDui");
                Game.CurrentGame.HandleCardDiscard(Owner, new List<Card>() { theCard });
            }
        }

        int MouKuiEffect;
        public MouKui()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard.Type is Sha; },
                Run1,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.CardUsageTargetConfirmed, trigger);

            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                Run2,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(ShaCancelling.PlayerShaTargetDodged, trigger2);

            var trigger3 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => 
                {
                    if (a.ReadonlyCard[MouKuiCheck] == 0)
                    {
                        a.ReadonlyCard[MouKuiCheck] = 1;
                        return;
                    }
                    a.ReadonlyCard[MouKuiCheck[a.Targets[0]]] >>= 1;
                }
                ,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(ShaCancelling.PlayerShaTargetShanModifier, trigger3);
            IsAutoInvoked = null;
        }

        static CardAttribute MouKuiCheck = CardAttribute.Register("MouKuiCheck");
    }
}

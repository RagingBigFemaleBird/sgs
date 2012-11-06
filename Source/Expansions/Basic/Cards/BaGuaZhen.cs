using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;

namespace Sanguosha.Expansions.Basic.Cards
{
    public class BaGuaZhen : Armor
    {
        class BaGuaZhenSkill : PassiveSkill
        {
            protected override void InstallTriggers(Player owner)
            {
                throw new NotImplementedException();
            }

            protected override void UninstallTriggers(Player owner)
            {
                throw new NotImplementedException();
            }
        }
        class BaGuaTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                {
                    return;
                }
                if (!((eventArgs.Card is CompositeCard) && ((eventArgs.Card as CompositeCard).Type is Shan)))
                {
                }
                int answer;
                if (Game.CurrentGame.UiProxies[Owner].AskForMultipleChoice(new MultipleChoicePrompt("BaGua"), Prompt.YesNoChoices, out answer) && answer == 0)
                {
                    ReadOnlyCard c = Game.CurrentGame.Judge(Owner, null, new Card() { Type = new BaGuaZhen() });
                    if (c.SuitColor == SuitColorType.Red)
                    {
                        eventArgs.Cards = new List<Card>();
                        ActionLog log = new ActionLog();
                        log.Source = Owner;
                        log.SkillAction = new BaGuaZhenSkill();
                        log.GameAction = GameAction.None;
                        Game.CurrentGame.NotificationProxy.NotifySkillUse(log);
                        throw new TriggerResultException(TriggerResult.Success);
                    }
                }
            }

            public BaGuaTrigger(Player p)
            {
                Owner = p;
            }
        }

        Trigger theTrigger;

        protected override void RegisterEquipmentTriggers(Player p)
        {
            theTrigger = new BaGuaTrigger(p) { Type = TriggerType.Card };
            Game.CurrentGame.RegisterTrigger(GameEvent.PlayerRequireCard, theTrigger);
        }

        protected override void UnregisterEquipmentTriggers(Player p)
        {
            Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerRequireCard, theTrigger);
        }
    }
}

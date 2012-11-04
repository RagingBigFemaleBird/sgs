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

namespace Sanguosha.Expansions.Basic.Skills
{
    /// <summary>
    /// 奸雄-每当你受到一次伤害后，你可以获得对你造成伤害的牌。
    /// </summary>
    public class JianXiong : PassiveSkill
    {
        class JianXiongTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source == null || eventArgs.Targets.IndexOf(Owner) < 0)
                {
                    return;
                }
                if (eventArgs.Cards == null || eventArgs.Cards.Count == 0)
                {
                    return;
                }
                int answer = 0;
                if (Game.CurrentGame.UiProxies[Owner].AskForMultipleChoice(new MultipleChoicePrompt("JianXiong", eventArgs.Source), Prompt.YesNoChoices, out answer) && answer == 0)
                {
                    Game.CurrentGame.HandleCardTransferToHand(null, Owner, new List<Card>(eventArgs.Cards));
                }
            }
            public JianXiongTrigger(Player p)
            {
                Owner = p;
            }
        }

        Trigger theTrigger;

        protected override void InstallTriggers(Sanguosha.Core.Players.Player owner)
        {
            theTrigger = new JianXiongTrigger(owner);
            Game.CurrentGame.RegisterTrigger(GameEvent.AfterDamageInflicted, theTrigger);
        }

        protected override void UninstallTriggers(Player owner)
        {
            if (theTrigger != null)
            {
                Game.CurrentGame.UnregisterTrigger(GameEvent.AfterDamageInflicted, theTrigger);
            }
        }
    }
}

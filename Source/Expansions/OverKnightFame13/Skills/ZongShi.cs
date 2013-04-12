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

namespace Sanguosha.Expansions.OverKnightFame13.Skills
{
    public class ZongShi2 : TriggerSkill
    {
        public ZongShi2()
        {
            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { var arg = a as PinDianCompleteEventArgs; if (arg != null && (arg.Source == p || arg.Targets.Contains(p))) return true; return false; },
                (p, e, a) =>
                {
                    var arg = a as PinDianCompleteEventArgs;
                    if (AskForSkillUse())
                    {
                        if (arg.Source == p)
                        {
                            if (arg.PinDianResult == true)
                            {
                                if (arg.CardsResult[1] == false)
                                {
                                    Game.CurrentGame.HandleCardTransferToHand(arg.Targets[0], arg.Source, new List<Card>() { arg.Cards[1] });
                                    arg.CardsResult[1] = true;
                                }
                            }
                            else
                            {
                                if (arg.CardsResult[0] == false)
                                {
                                    arg.CardsResult[0] = true;
                                }
                            }
                        }
                        else /*is target*/
                        {
                            if (arg.PinDianResult == true)
                            {
                                if (arg.CardsResult[0] == false)
                                {
                                    Game.CurrentGame.HandleCardTransferToHand(arg.Source, arg.Targets[0], new List<Card>() { arg.Cards[0] });
                                    arg.CardsResult[0] = true;
                                }
                            }
                            else
                            {
                                if (arg.CardsResult[1] == false)
                                {
                                    arg.CardsResult[1] = true;
                                }
                            }
                        }
                    }
                },
                TriggerCondition.Global
            ) {IsAutoNotify = false, AskForConfirmation = false };
            Triggers.Add(GameEvent.PinDianComplete, trigger2);
            IsEnforced = true;
        }

    }
}

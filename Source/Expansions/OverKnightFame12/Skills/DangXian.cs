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

namespace Sanguosha.Expansions.OverKnightFame12.Skills
{
    /// <summary>
    /// 当先-锁定技，回合开始时，你执行一个额外的出牌阶段。
    /// </summary>
    public class DangXian : TriggerSkill
    {
        public DangXian()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) =>
                {
                    var saveP = Game.CurrentGame.CurrentPlayer;
                    var savePh = Game.CurrentGame.CurrentPhase;
                    var savePhI = Game.CurrentGame.CurrentPhaseEventIndex;

                    Game.CurrentGame.CurrentPhaseEventIndex = 0;
                    Game.CurrentGame.CurrentPhase = TurnPhase.Play;
                    do
                    {
                        Player currentPlayer = Game.CurrentGame.CurrentPlayer;
                        GameEventArgs args = new GameEventArgs() { Source = currentPlayer};
                        Trace.TraceInformation("Main game loop running {0}:{1}", currentPlayer.Id, Game.CurrentGame.CurrentPhase);
                        try
                        {
                            var phaseEvent = Game.PhaseEvents[Game.CurrentGame.CurrentPhaseEventIndex];
                            if (phaseEvent.ContainsKey(Game.CurrentGame.CurrentPhase))
                            {
                                Game.CurrentGame.Emit(Game.PhaseEvents[Game.CurrentGame.CurrentPhaseEventIndex][Game.CurrentGame.CurrentPhase], args);
                            }
                        }
                        catch (TriggerResultException ex)
                        {
                            if (ex.Status == TriggerResult.End)
                            {
                            }
                        }

                        Game.CurrentGame.CurrentPhaseEventIndex++;
                        if (Game.CurrentGame.CurrentPhaseEventIndex >= Game.PhaseEvents.Length - 1 || currentPlayer.IsDead)
                        {
                            Game.CurrentGame.CurrentPhaseEventIndex = 0;
                            Game.CurrentGame.CurrentPhase++;
                            if ((int)Game.CurrentGame.CurrentPhase >= Enum.GetValues(typeof(TurnPhase)).Length || (int)Game.CurrentGame.CurrentPhase < 0 || currentPlayer.IsDead)
                            {
                                break;
                            }
                        }
                    } while (Game.CurrentGame.CurrentPhaseEventIndex < 3 && Game.CurrentGame.CurrentPhase == TurnPhase.Play);

                    
                    Game.CurrentGame.CurrentPlayer = saveP;
                    Game.CurrentGame.CurrentPhase = savePh;
                    Game.CurrentGame.CurrentPhaseEventIndex = savePhI;
                },
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false };
            Triggers.Add(GameEvent.PhaseProceedEvents[TurnPhase.BeforeStart], trigger);
            IsEnforced = true;
        }
    }
}

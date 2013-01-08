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
    /// 洛神-回合开始阶段开始时，你可以进行一次判定：若结果为黑色（通常是完全不可能的），你获得此牌；你可以重复此流程，直到出现红色的判定结果为止。
    /// </summary>

    class LuoShenGetCard : GetJudgeCardTrigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (eventArgs.Cards.Count > 0 && eventArgs.Cards[0].SuitColor == SuitColorType.Black)
                base.Run(gameEvent, eventArgs);
            else
                Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerJudgeDone, this); 
        }

        public LuoShenGetCard(Player p, ISkill skill, ICard card, bool permenant = false) :
            base(p, skill, card, permenant)
        { }
    }

    public class LuoShen : TriggerSkill
    {
        void OnPhaseBegin(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (AskForSkillUse())
            {
                ReadOnlyCard c;
                do
                {
                    Game.CurrentGame.RegisterTrigger(GameEvent.PlayerJudgeDone, new LuoShenGetCard(Owner, this, null) { Priority = int.MinValue });
                    c = Game.CurrentGame.Judge(Owner, this, null, (judgeResultCard) => { return judgeResultCard.SuitColor == SuitColorType.Black; });
                } while (c.SuitColor == SuitColorType.Black && AskForSkillUse());
            }
        }

        public LuoShen()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return true; },
                OnPhaseBegin,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false };
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.Start], trigger);
            IsAutoInvoked = true;
        }

    }
}

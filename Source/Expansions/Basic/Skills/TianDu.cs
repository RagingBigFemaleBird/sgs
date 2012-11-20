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
    /// 天妒-在你的判定牌生效后，你可以获得此牌。
    /// </summary>
    public class TianDu : TriggerSkill
    {
        void GetMyCard(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            Game.CurrentGame.HandleCardTransferToHand(Owner, Owner, new List<Card>(Game.CurrentGame.Decks[eventArgs.Source, DeckType.JudgeResult]));
        }


        public TianDu()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Game.CurrentGame.Decks[a.Source, DeckType.JudgeResult].Count > 0; },
                GetMyCard,
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PlayerJudgeDone, trigger);
            IsAutoInvoked = true;
        }
    }
}

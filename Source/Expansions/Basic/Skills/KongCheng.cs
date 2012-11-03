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
    /// 空城-锁定技，若你没有手牌，你不能成为【杀】或【决斗】的目标。
    /// </summary>
    public class KongCheng : PassiveSkill
    {
        class KongChengTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Trace.Assert(eventArgs != null);
                if (eventArgs.Targets.IndexOf(Owner) < 0)
                {
                    return;
                }
                if (((eventArgs.Card.Type is Sha) || (eventArgs.Card.Type is JueDou))
                    && (Game.CurrentGame.Decks[Owner, DeckType.Hand].Count == 0))
                {
                    throw new TriggerResultException(TriggerResult.Fail);
                }
                return;
            }
            public KongChengTrigger(Player p)
            {
                Owner = p;
            }
        }

        Trigger theTrigger;

        protected override void InstallTriggers(Sanguosha.Core.Players.Player owner)
        {
            theTrigger = new KongChengTrigger(owner);
            Game.CurrentGame.RegisterTrigger(GameEvent.PlayerCanBeTargeted, theTrigger);
        }

        protected override void UninstallTriggers(Player owner)
        {
            if (theTrigger != null)
            {
                Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerCanBeTargeted, theTrigger);
            }
        }
        public override bool isEnforced
        {
            get
            {
                return true;
            }
        }
    }
}

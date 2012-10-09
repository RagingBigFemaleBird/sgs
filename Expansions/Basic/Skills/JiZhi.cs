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
    /// 集智-当你使用一张非延时类锦囊牌时，你可以摸一张牌。
    /// </summary>
    public class JiZhi : PassiveSkill
    {
        class JiZhiTrigger : Trigger
        {
            public Player Owner { get; set; }
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Trace.Assert(eventArgs != null);
                if (eventArgs.Source != Owner)
                {
                    return;
                }
                if (CardCategoryManager.IsCardCategory(eventArgs.Card.Type.Category, CardCategory.ImmediateTool))
                {
                    Game.CurrentGame.DrawCards(Owner, 1);
                }
                return;
            }
            public JiZhiTrigger(Player p)
            {
                Owner = p;
            }
        }

        protected override void InstallTriggers(Sanguosha.Core.Players.Player owner)
        {
            Game.CurrentGame.RegisterTrigger(GameEvent.PlayerPlayedCard, new JiZhiTrigger(owner));
        }
    }
}

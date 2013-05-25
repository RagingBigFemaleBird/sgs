using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using System.Diagnostics;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.OverKnightFame13.Skills
{
    /// <summary>
    /// 仁心-每当一名其他角色处于濒死状态时，你可以将武将牌翻面并将所有手牌（至少一张）交给该角色。若如此做，该角色回复1点体力。
    /// </summary>
    public class RenXin : TriggerSkill
    {
        class RenXinVerifier : CardsAndTargetsVerifier
        {
            public RenXinVerifier()
            {
                MinCards = 1;
                MaxPlayers = 1;
                MinPlayers = 1;
                Helper.NoCardReveal = true;
            }

            protected override bool VerifyPlayer(Player source, Player player)
            {
                return source != player;
            }
        }

        public RenXin()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return !a.Targets.Contains(p) && p.HandCards().Count > 0; },
                (p, e, a) =>
                {
                    var target = a.Targets[0];
                    NotifySkillUse(new List<Player>() { target });
                    Game.CurrentGame.HandleCardTransferToHand(p, target, new List<Card>(p.HandCards()));
                    p.IsImprisoned = !p.IsImprisoned;
                    Game.CurrentGame.RecoverHealth(p, target, 1);
                },
                TriggerCondition.Global
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.PlayerIsAboutToDie, trigger);
            IsAutoInvoked = false;
        }
    }
}

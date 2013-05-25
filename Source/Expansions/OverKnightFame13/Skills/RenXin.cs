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
    /// 冰心-每当你进入濒死状态时，你可以将所有牌以任意方式交给任意数量的其他角色，若如此做，你将武将牌翻面。
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
                (p, e, a) => { return !a.Targets.Contains(p) && p.HandCards().Count + p.Equipments().Count > 0; },
                (p, e, a) =>
                {
                    var target = a.Targets[0];
                    if (AskForSkillUse())
                    {
                        NotifySkillUse(new List<Player>() { target });
                        Game.CurrentGame.HandleCardTransferToHand(p, target, new List<Card>(p.HandCards()));
                        p.IsImprisoned = !p.IsImprisoned;
                    }
                },
                TriggerCondition.Global
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PlayerIsAboutToDie, trigger);
            IsAutoInvoked = null;
        }
    }
}

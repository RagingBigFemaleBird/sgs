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
    public class BingXin : TriggerSkill
    {
        class BingXinVerifier : CardsAndTargetsVerifier
        {
            public BingXinVerifier()
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

        public BingXin()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return p.HandCards().Count + p.Equipments().Count > 0; },
                (p, e, a) =>
                {
                    ISkill skill;
                    List<Card> cards;
                    List<Player> players;
                    bool invoke = false;
                    while (p.HandCards().Count + p.Equipments().Count > 0)
                    {
                        if (p.AskForCardUsage(new CardUsagePrompt("BingXin", this), new BingXinVerifier(), out skill, out cards, out players))
                        {
                            NotifySkillUse(players);
                            Game.CurrentGame.HandleCardTransferToHand(p, players[0], cards);
                            invoke = true;
                        }
                        else if (invoke)
                        {
                            List<Player> temp = Game.CurrentGame.AlivePlayers;
                            temp.Remove(p);
                            Player target = temp.First();
                            Game.CurrentGame.HandleCardTransferToHand(p, target, p.HandCards().Concat(p.Equipments()).ToList());
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (invoke)
                    {
                        p.IsImprisoned = !p.IsImprisoned;
                    }
                },
                TriggerCondition.OwnerIsTarget
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PlayerIsAboutToDie, trigger);
            IsAutoInvoked = null;
        }
    }
}

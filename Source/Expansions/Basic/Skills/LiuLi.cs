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
    /// 流离-当你成为【杀】的目标时，你可以弃置一张牌将此【杀】转移给你攻击范围内的一名其他角色，该角色不得是此【杀】的使用者。
    /// </summary>
    public class LiuLi : TriggerSkill
    {
        class LiuLiVerifier : CardUsageVerifier
        {
            public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if (skill != null)
                {
                    return VerifierResult.Fail;
                }
                if (players != null && players.Count > 1)
                {
                    return VerifierResult.Fail;
                }
                if (players != null && players.Count != 0 && (players[0] == source || players[0] == ShaSource))
                {
                    return VerifierResult.Fail;
                }
                if (cards != null && cards.Count > 1)
                {
                    return VerifierResult.Fail;
                }
                if ((cards == null || cards.Count == 0) && players != null && players.Count > 0)
                {
                    // if we have players but we don't have cards. it's not allowed
                    return VerifierResult.Fail;
                }
                if (cards == null || cards.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (!Game.CurrentGame.PlayerCanDiscardCard(source, cards[0]))
                {
                    return VerifierResult.Fail;
                }
                CardHandler handler = new Sha();
                handler.HoldInTemp(cards);
                if (players != null && players.Count != 0 && Game.CurrentGame.DistanceTo(source, players[0]) > source[Player.AttackRange] + 1)
                {
                    handler.ReleaseHoldInTemp();
                    return VerifierResult.Fail;
                }
                handler.ReleaseHoldInTemp();
                if (players == null || players.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                return VerifierResult.Success;
            }

            public override IList<CardHandler> AcceptableCardTypes
            {
                get { return null; }
            }

            Player ShaSource;
            public LiuLiVerifier(Player p)
            {
                ShaSource = p;
            }
        }

        public void OnPlayerIsCardTarget(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (Game.CurrentGame.UiProxies[owner].AskForCardUsage(new CardUsagePrompt("LiuLi"), new LiuLiVerifier(eventArgs.Source), out skill, out cards, out players))
            {
                Game.CurrentGame.HandleCardDiscard(players[0], cards);
                eventArgs.Targets.Remove(owner);
                eventArgs.Targets.Add(players[0]);
            }
        }

        public LiuLi()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Card.Type is Sha; },
                OnPlayerIsCardTarget,
                TriggerCondition.OwnerIsTarget
            ) { Priority = SkillPriority.LiuLi };
            Triggers.Add(GameEvent.CardUsageTargetConfirming, trigger);
        }
    }
}

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
    public class LiuLi : PassiveSkill
    {
        class LiuLiTrigger : Trigger
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
                    if (players != null && players.Count != 0 && Game.CurrentGame.DistanceTo(source, players[0]) > players[0][Player.AttackRange] + 1)
                    {
                        return VerifierResult.Fail;
                    }
                    if (cards != null && cards.Count > 1)
                    {
                        return VerifierResult.Fail;
                    }
                    if (cards == null || cards.Count == 0)
                    {
                        return VerifierResult.Partial;
                    }
                    if (players == null || players.Count == 0)
                    {
                        return VerifierResult.Partial;
                    }
                    return VerifierResult.Success;
                }

                public override IList<CardHandler> AcceptableCardType
                {
                    get { return new List<CardHandler>(); }
                }

                Player ShaSource;
                public LiuLiVerifier(Player p)
                {
                    ShaSource = p;
                }
            }

            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Trace.Assert(eventArgs.Targets.Count == 1);
                if (!eventArgs.Targets.Contains(Owner))
                {
                    return;
                }
                if (!(eventArgs.Card.Type is Sha))
                {
                    return;
                }
                ISkill skill;
                List<Card> cards;
                List<Player> players;
                if (Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("LiuLi"), new LiuLiVerifier(eventArgs.Source), out skill, out cards, out players))
                {
                    Game.CurrentGame.HandleCardDiscard(players[0], cards);
                    eventArgs.Targets = new List<Player>(players);
                    throw new TriggerResultException(TriggerResult.Retry);
                }
                return;
            }
            public LiuLiTrigger(Player p)
            {
                Owner = p;
            }
        }

        Trigger theTrigger;

        protected override void InstallTriggers(Sanguosha.Core.Players.Player owner)
        {
            theTrigger = new LiuLiTrigger(owner);
            theTrigger.Priority = int.MaxValue;
            Game.CurrentGame.RegisterTrigger(GameEvent.PlayerIsCardTarget, theTrigger);
        }

        protected override void UninstallTriggers(Player owner)
        {
            if (theTrigger != null)
            {
                Game.CurrentGame.UnregisterTrigger(GameEvent.PlayerIsCardTarget, theTrigger);
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

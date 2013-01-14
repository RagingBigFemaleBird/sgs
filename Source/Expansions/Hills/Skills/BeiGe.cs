using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using System.Diagnostics;

namespace Sanguosha.Expansions.Hills.Skills
{
    /// <summary>
    /// 悲歌-每当一名角色每受到【杀】造成的一次伤害后，你可以弃置一张牌，并唱歌。
    /// </summary>
    public class BeiGe : TriggerSkill
    {
        public class BeiGeVerifier : ICardUsageVerifier
        {
            public UiHelper Helper { get { return new UiHelper(); } }
            public VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if (skill != null || (players != null && players.Count != 0))
                {
                    return VerifierResult.Fail;
                }
                if (cards == null || cards.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (cards.Count > 1)
                {
                    return VerifierResult.Fail;
                }
                if (!Game.CurrentGame.PlayerCanDiscardCard(source, cards[0]))
                {
                    return VerifierResult.Fail;
                }
                return VerifierResult.Success;
            }

            public IList<CardHandler> AcceptableCardTypes
            {
                get { throw new NotImplementedException(); }
            }

            public VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                return FastVerify(source, skill, cards, players);
            }
        }

        public void OnAfterDamageInflicted(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("BeiGe", eventArgs.Source), new BeiGeVerifier(), out skill, out cards, out players))
            {
                NotifySkillUse(players);
                Game.CurrentGame.HandleCardDiscard(owner, cards);
                var result = Game.CurrentGame.Judge(eventArgs.Source, this);
                switch (result.Suit)
                {
                    case SuitType.Club:
                        if (eventArgs.Source != null && !eventArgs.Source.IsDead)
                        {
                            Game.CurrentGame.ForcePlayerDiscard(eventArgs.Source,
                                (p, i) =>
                                {
                                    return 2 - i;
                                },
                                true);
                        }
                        break;
                    case SuitType.Diamond:
                        Game.CurrentGame.DrawCards(eventArgs.Targets[0], 2);
                        break;
                    case SuitType.Heart:
                        Game.CurrentGame.RecoverHealth(owner, eventArgs.Targets[0], 1);
                        break;
                    case SuitType.Spade:
                        if (eventArgs.Source != null && !eventArgs.Source.IsDead)
                        {
                            eventArgs.Source.IsImprisoned = !eventArgs.Source.IsImprisoned;
                        }
                        break;
                    default:
                        Trace.Assert(false);
                        break;
                }

            }
        }

        public BeiGe()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.ReadonlyCard != null && (a.ReadonlyCard.Type is Sha) && !a.Targets[0].IsDead; },
                OnAfterDamageInflicted,
                TriggerCondition.Global
            ) { IsAutoNotify = false, AskForConfirmation = false };
            Triggers.Add(GameEvent.AfterDamageInflicted, trigger);
            IsAutoInvoked = null;
        }
    }
}

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
    /// 刚烈-每当你受到一次伤害后，你可以进行一次判定，若结果不为红桃，则伤害来源选择一项：弃置两张手牌，或受到你对其造成的1点伤害。
    /// </summary>
    public class GangLie : TriggerSkill
    {
        public class GangLieVerifier : ICardUsageVerifier
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
                if (cards.Count > 2)
                {
                    return VerifierResult.Fail;
                }
                foreach (Card c in cards)
                {
                    if (!Game.CurrentGame.PlayerCanDiscardCard(source, c))
                    {
                        return VerifierResult.Fail;
                    }
                    if (c.Place.DeckType != DeckType.Hand)
                    {
                        return VerifierResult.Fail;
                    }
                }
                if (cards.Count < 2)
                {
                    return VerifierResult.Partial;
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
            ReadOnlyCard c = Game.CurrentGame.Judge(owner, this);
            if (c.Suit != SuitType.Heart)
            {
                NotifySkillUse(new List<Player>() { eventArgs.Source });
                List<DeckPlace> deck = new List<DeckPlace>();
                GangLieVerifier ver = new GangLieVerifier();
                ISkill skill;
                List<Card> cards;
                List<Player> players;
                if (!Game.CurrentGame.UiProxies[eventArgs.Source].AskForCardUsage(new CardUsagePrompt("GangLie", Owner), ver, out skill, out cards, out players))
                {
                    Game.CurrentGame.DoDamage(owner, eventArgs.Source, 1, DamageElement.None, null, null);
                }
                else
                {
                    Game.CurrentGame.HandleCardDiscard(eventArgs.Source, cards);
                }
            }
            else
            {
                Trace.TraceInformation("Judgement fail");
            }
        }

        public GangLie()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return a.Source != null; },
                OnAfterDamageInflicted,
                TriggerCondition.OwnerIsTarget
            ) { IsAutoNotify = false };
            Triggers.Add(GameEvent.AfterDamageInflicted, trigger);
            IsAutoInvoked = null;
        }
    }
}

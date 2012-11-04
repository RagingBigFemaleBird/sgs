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
    public class GangLie : PassiveSkill
    {
        class GangLieTrigger : Trigger
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

                public IList<CardHandler> AcceptableCardType
                {
                    get { throw new NotImplementedException(); }
                }

                public VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players)
                {
                    return FastVerify(source, skill, cards, players);
                }
            }

            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source == null || eventArgs.Targets.IndexOf(Owner) < 0)
                {
                    return;
                }
                int answer = 0;
                if (Game.CurrentGame.UiProxies[Owner].AskForMultipleChoice(new MultipleChoicePrompt("GangLie", eventArgs.Source), Prompt.YesNoChoices, out answer) && answer == 0)
                {
                    SymbolicCard c = Game.CurrentGame.Judge(Owner);
                    if (c.Suit != SuitType.Heart)
                    {
                        List<DeckPlace> deck = new List<DeckPlace>();
                        GangLieVerifier ver = new GangLieVerifier();
                        ISkill skill;
                        List<Card> cards;
                        List<Player> players;
                        if (!Game.CurrentGame.UiProxies[eventArgs.Source].AskForCardUsage(new CardUsagePrompt("GangLie", Owner), ver, out skill, out cards, out players))
                        {
                            Game.CurrentGame.DoDamage(Owner, eventArgs.Source, 1, DamageElement.None, null);
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
            }
            public GangLieTrigger(Player p)
            {
                Owner = p;
            }
        }

        Trigger theTrigger;

        protected override void InstallTriggers(Sanguosha.Core.Players.Player owner)
        {
            theTrigger = new GangLieTrigger(owner); 
            Game.CurrentGame.RegisterTrigger(GameEvent.AfterDamageInflicted, theTrigger);
        }

        protected override void UninstallTriggers(Player owner)
        {
            if (theTrigger != null)
            {
                Game.CurrentGame.UnregisterTrigger(GameEvent.AfterDamageInflicted, theTrigger);
            }
        }
    }
}

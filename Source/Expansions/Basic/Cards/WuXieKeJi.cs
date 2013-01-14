using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;

namespace Sanguosha.Expansions.Basic.Cards
{
    
    public class WuXieKeJi : CardHandler
    {
        public static readonly CardAttribute CannotBeCountered = CardAttribute.Register("CannotBeCountered");

        protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
        {
            throw new NotImplementedException();
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            return VerifierResult.Fail;
        }

        public override CardCategory Category
        {
            get { return CardCategory.ImmediateTool; }
        }
    }

    
    public class WuXieKeJiTrigger : Trigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ReadOnlyCard card = eventArgs.ReadonlyCard;
            SingleCardUsageVerifier v1 = new SingleCardUsageVerifier((c) => { return c.Type is WuXieKeJi; }, true, new WuXieKeJi());
            List<Card> cards;
            List<Player> players;
            ISkill skill;
            Player responder;
            bool WuXieSuccess = false;
            Trace.Assert(eventArgs.Targets.Count == 1);
            Player promptPlayer = eventArgs.Targets[0];
            ICard promptCard = eventArgs.ReadonlyCard;
            if (card != null && CardCategoryManager.IsCardCategory(card.Type.Category, CardCategory.Tool) &&
                card[WuXieKeJi.CannotBeCountered] == 0 && card[WuXieKeJi.CannotBeCountered[promptPlayer]] == 0)
            {
                bool askWuXie = false;
                foreach (var p in Game.CurrentGame.AlivePlayers)
                {
                    foreach (var c in Game.CurrentGame.Decks[p, DeckType.Hand])
                    {
                        if (c.Type is WuXieKeJi)
                        {
                            askWuXie = true;
                            break;
                        }
                    }
                    foreach (var sk in p.ActionableSkills)
                    {
                        CardTransformSkill cts = sk as CardTransformSkill;
                        if (cts != null)
                        {
                            if (cts.PossibleResults == null)
                            {
                                askWuXie = true;
                                break;
                            }
                            foreach (var pr in cts.PossibleResults)
                            {
                                if (pr is WuXieKeJi)
                                {
                                    askWuXie = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (askWuXie) break;
                }
                Game.CurrentGame.SyncConfirmationStatus(ref askWuXie);
                if (!askWuXie) return;
                while (true)
                {
                    Prompt prompt = new CardUsagePrompt("WuXieKeJi", promptPlayer, promptCard);
                    if (Game.CurrentGame.GlobalProxy.AskForCardUsage(
                        prompt, v1, out skill, out cards, out players, out responder))
                    {
                        try
                        {
                            GameEventArgs args = new GameEventArgs();
                            args.Source = responder;
                            args.Targets = players;
                            args.Skill = skill;
                            args.Cards = cards;
                            Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
                        }
                        catch (TriggerResultException e)
                        {
                            Trace.Assert(e.Status == TriggerResult.Retry);
                            continue;
                        }
                        promptPlayer = responder;
                        promptCard = new CompositeCard();
                        promptCard.Type = new WuXieKeJi();
                        (promptCard as CompositeCard).Subcards = null;
                        WuXieSuccess = !WuXieSuccess;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (WuXieSuccess)
            {
                throw new TriggerResultException(TriggerResult.End);
            }
        }
    }
}

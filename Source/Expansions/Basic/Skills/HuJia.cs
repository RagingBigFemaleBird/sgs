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
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Expansions.Basic.Skills
{
    /// <summary>
    /// 护驾―主公技，当你需要使用或打出一张【闪】时，你可令其他魏势力角色打出一张【闪】(视为由你使用或打出)。
    /// </summary>
    public class HuJia : TriggerSkill
    {
        void CallOfShan(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ICard result = null;
            List<Player> toProcess = new List<Player>(Game.CurrentGame.AlivePlayers);
            toProcess.Remove(Owner);
            Game.CurrentGame.SortByOrderOfComputation(Owner, toProcess);
            bool noAnswer = true;
            foreach (var player in toProcess)
            {
                if (player.Hero.Allegiance == Core.Heroes.Allegiance.Wei)
                {
                    bool failToRespond = false;
                    GameEventArgs args = new GameEventArgs();
                    args.Source = player;
                    args.Targets = eventArgs.Targets;
                    args.Card = new CompositeCard();
                    args.Card.Type = new Shan();
                    try
                    {
                        Game.CurrentGame.Emit(GameEvent.PlayerRequireCard, args);
                    }
                    catch (TriggerResultException e)
                    {
                        if (e.Status == TriggerResult.Success)
                        {
                            eventArgs.Cards = new List<Card>(args.Cards);
                            throw new TriggerResultException(TriggerResult.Success);
                        }
                    }
                    while (true)
                    {
                        IUiProxy ui = Game.CurrentGame.UiProxies[player];
                        SingleCardUsageVerifier v1 = new SingleCardUsageVerifier((c) => { return c.Type is Shan; });
                        ISkill skill;
                        List<Player> p;
                        List<Card> cards;
                        if (!ui.AskForCardUsage(new CardUsagePrompt("HuJia", Owner), v1, out skill, out cards, out p))
                        {
                            failToRespond = true;
                            break;
                        }
                        if (!Game.CurrentGame.CommitCardTransform(player, skill, cards, out result, eventArgs.Targets))
                        {
                            continue;
                        }
                        noAnswer = false;
                        Trace.TraceInformation("Player {0} Responded HuJia with SHAN, ", player.Id);
                        break;
                    }
                    if (failToRespond)
                    {
                        continue;
                    }
                    break;
                }
            }

            if (noAnswer)
            {
                return;
            }

            Trace.Assert(result != null);
            eventArgs.Cards = new List<Card>();
            if (result is CompositeCard)
            {
                eventArgs.Cards.AddRange(((CompositeCard)result).Subcards);
            }
            else
            {
                Trace.Assert(result is Card);
                eventArgs.Cards.Add((Card)result);
            }
            throw new TriggerResultException(TriggerResult.Success);
        }

        bool CanHuJia(Player p, GameEvent e, GameEventArgs a)
        {
            if (!((a.Card is CompositeCard) && ((a.Card as CompositeCard).Type is Shan)))
            {
                return false;
            }
            List<Player> players = new List<Player>(Game.CurrentGame.AlivePlayers);
            bool hasWei = false;
            foreach (Player pp in players)
            {
                if (p != pp && pp.Allegiance == Core.Heroes.Allegiance.Wei)
                {
                    hasWei = true;
                    break;
                }
            }
            return hasWei;
        }

        public HuJia()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                CanHuJia,
                CallOfShan,
                TriggerCondition.OwnerIsSource
            );
            Triggers.Add(GameEvent.PlayerRequireCard, trigger);
        }
        public override bool IsRulerOnly
        {
            get
            {
                return true;
            }
        }
    }
}

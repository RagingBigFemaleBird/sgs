using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Cards;

namespace Sanguosha.Core.Triggers
{
    public delegate bool TriggerPredicate(Player player, GameEvent gameEvent, GameEventArgs args);

    public delegate void TriggerAction(Player owner, GameEvent gameEvent, GameEventArgs args);

    public enum TriggerCondition
    {
        Global = (1 << 0),
        SourceHasCards = (1 << 1),
        SourceHasHandCards = (1 << 2),
        SourceHasNoHandCards = (1 << 3),
        OwnerHasNoHandCards = (1 << 4),
        OwnerIsSource = (1 << 5),
        OwnerIsTarget = (1 << 6),
    }

    public class RelayTrigger : Trigger
    {
        public RelayTrigger() { }

        private TriggerAction execute;

        public TriggerAction Execute
        {
            get { return execute; }
            set { execute = value; }
        }

        private TriggerPredicate canExecute;

        public TriggerPredicate CanExecute
        {
            get { return canExecute; }
            set { canExecute = value; }
        }

        private TriggerCondition triggerCondition;

        public TriggerCondition Condition
        {
            get { return triggerCondition; }
            set { triggerCondition = value; }
        }

        public RelayTrigger(TriggerPredicate predicate, TriggerAction action, TriggerCondition condition)
        {
            CanExecute = predicate;
            Execute = action;
            Condition = condition;
        }

        public RelayTrigger(TriggerAction action, TriggerCondition condition)
        {
            CanExecute = (p, a, e) => { return true; };
            Execute = action;
            Condition = condition;
        }

        private bool CheckCondition(TriggerCondition checkAgainst)
        {
            return (Condition & checkAgainst) == checkAgainst;
        }

        public bool CheckConditions(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (CheckCondition(TriggerCondition.OwnerIsSource)
                && (eventArgs.Source == null || eventArgs.Source != Owner))
            {
                return false;
            }
            else if (CheckCondition(TriggerCondition.OwnerIsTarget)
                     && (eventArgs.Targets == null || !eventArgs.Targets.Contains(Owner)))
            {
                return false;
            }
            else if (CheckCondition(TriggerCondition.SourceHasCards)
                     && (Game.CurrentGame.Decks[eventArgs.Source, DeckType.Hand].Count == 0 &&
                        Game.CurrentGame.Decks[eventArgs.Source, DeckType.Equipment].Count == 0))
            {
                return false;
            }
            else if (CheckCondition(TriggerCondition.SourceHasHandCards) && (Game.CurrentGame.Decks[eventArgs.Source, DeckType.Hand].Count == 0))
            {
                return false;
            }
            else if (CheckCondition(TriggerCondition.SourceHasNoHandCards) && (Game.CurrentGame.Decks[eventArgs.Source, DeckType.Hand].Count > 0))
            {
                return false;
            }
            else if (CheckCondition(TriggerCondition.OwnerHasNoHandCards) && (Game.CurrentGame.Decks[Owner, DeckType.Hand].Count > 0))
            {
                return false;
            }
            return true;
        }


        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            if (!CheckConditions(gameEvent, eventArgs)) return;
            if (CanExecute(Owner, gameEvent, eventArgs))
            {
                if (Execute != null)
                {
                    Execute(Owner, gameEvent, eventArgs);
                }
            }
        }
    }
}

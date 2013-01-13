using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Games;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Players;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Skills;
using Sanguosha.Core.UI;

namespace Sanguosha.Core.Triggers
{
    public class GameEventArgs
    {
        public GameEventArgs()
        {
            Targets = new List<Player>();
            Cards = new List<Card>();
        }

        public void CopyFrom(GameEventArgs another)
        {
            source = another.source;
            targets = new List<Player>(another.targets);
            cards = new List<Card>(another.cards);
            card = another.card;
            readonlyCard = another.readonlyCard;
        }

        private Player source;

        public Player Source
        {
            get { return source; }
            set { source = value; }
        }

        private List<Player> targets;

        public List<Player> Targets
        {
            get { return targets; }
            set { targets = value; }
        }

        private List<Player> uiTargets;

        public List<Player> UiTargets
        {
            get { return uiTargets; }
            set { uiTargets = value; }
        }

        private List<Card> cards;

        public List<Card> Cards
        {
            get { return cards; }
            set { cards = value; }
        }

        private ISkill skill;

        public ISkill Skill
        {
            get { return skill; }
            set { skill = value; }
        }

        private ICard card;

        public ICard Card
        {
            get { return card; }
            set { card = value; }
        }

        private GameEventArgs inResponseTo;

        /// <summary>
        /// Gets/sets the game event(arg) that this game event(arg) is responding to.
        /// </summary>
        /// <remarks>
        /// 仅在闪用于抵消杀以及无懈可击用于抵消锦囊时被设置
        /// </remarks>
        public GameEventArgs InResponseTo
        {
            get { return inResponseTo; }
            set { inResponseTo = value; }
        }

        private ReadOnlyCard readonlyCard;

        public ReadOnlyCard ReadonlyCard
        {
            get { return readonlyCard; }
            set { readonlyCard = value; }
        }
    }

    public class HealthChangedEventArgs : GameEventArgs
    {
        public HealthChangedEventArgs() { }

        public HealthChangedEventArgs(DamageEventArgs args)
        {
            CopyFrom(args);
            Delta = -args.Magnitude;
        }

        /// <summary>
        /// Gets/sets the health change value.
        /// </summary>
        public int Delta
        {
            get;
            set;
        }
    }

    public class DamageEventArgs : GameEventArgs
    {
        public Player OriginalTarget
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/sets the magnitude of damage
        /// </summary>
        public int Magnitude
        {
            get;
            set;
        }

        public DamageElement Element
        {
            get;
            set;
        }

    }

    public class DiscardCardEventArgs : GameEventArgs
    {
        public DiscardReason Reason { get; set; }
    }

    public class AdjustmentEventArgs : GameEventArgs
    {
        public int AdjustmentAmount { get; set; }
        public int OriginalAmount { get; set; }
    }

    public class SkillSetChangedEventArgs : GameEventArgs
    {
        public SkillSetChangedEventArgs()
        {
            Skills = new List<ISkill>();
        }

        public bool IsLosingSkill
        {
            get;
            set;
        }

        public List<ISkill> Skills
        {
            get;
            set;
        }
    }
}

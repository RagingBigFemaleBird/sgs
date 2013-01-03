using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;

namespace Sanguosha.Core.UI
{
    public interface IGameLog
    {
    }

    public class CardUseLog : IGameLog
    {
        public ISkill Skill { get; set; }
        public List<Card> Cards { get; set; }
        public Player Source { get; set; }
        public List<Player> Targets { get; set; }
        public CardHandler Type { get; set; }
        public Player Target
        {
            get { Trace.Assert(Targets.Count == 1); return Targets[0]; }
            set { Trace.Assert(Targets == null); Targets = new List<Player>(); Targets.Add(value); }
        }
    }

    public class DamageLog : IGameLog
    {
        public Player Source { get; set; }
        public Player Target { get; set; }
        int Delta { get; set; }
        DamageElement Element { get; set; }
        bool HealthLost { get; set; }
    }

    public class HpRecoveryLog : IGameLog
    {
        public Player Source { get; set; }
        public Player Target { get; set; }
        int Delta { get; set; }
    }

    public class HpLostLog : IGameLog
    {
        public Player Player { get; set; }
        int Delta { get; set; }
    }

    public enum GameAction
    {
        None,
        Play,
        Use,
        Discard,
        PlaceIntoDiscard,
        Show,
        Judge,
    }

    public class ActionLog : IGameLog
    {
        public ActionLog()
        {
            Targets = new List<Player>();
            SecondaryTargets = new List<Player>();
        }
        public GameAction GameAction { get; set; }
        public ISkill SkillAction { get; set; }
        
        /// <summary>
        /// Gets/sets skill tag that is used for indexing different types of animation/audio
        /// associated with the same skill.
        /// </summary>
        public int SkillTag { get; set; }

        public ICard CardAction { get; set; }
        public Player Source { get; set; }
        public List<Player> Targets { get; set; }
        public List<Player> SecondaryTargets { get; set; }
    }
}

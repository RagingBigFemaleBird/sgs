using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Games;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Players;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Skills;

namespace Sanguosha.Core.Triggers
{
    [Serializable]
    public class GameEventArgs
    {
        private Player source;

        public Player Source
        {
            get { return source; }
            set { source = value; }
        }

        private Player target;

        public Player Target
        {
            get { return target; }
            set { target = value; }
        }
        private List<Player> targets;

        public List<Player> Targets
        {
            get { return targets; }
            set { targets = value; }
        }

        private Game game;

        public Game Game
        {
            get { return game; }
            set { game = value; }
        }

        private int intArg;

        public int IntArg
        {
            get { return intArg; }
            set { intArg = value; }
        }

        private int intArg2;

        public int IntArg2
        {
            get { return intArg2; }
            set { intArg2 = value; }
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

    }
}

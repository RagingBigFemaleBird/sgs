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

        public int IntArg { get; set; }
        public int IntArg2 { get; set; }
        public int IntArg3 { get; set; }

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

        private string stringArg;

        public string StringArg
        {
            get { return stringArg; }
            set { stringArg = value; }
        }

        private ICard card;

        public ICard Card
        {
            get { return card; }
            set { card = value; }
        }

        private ICard extraCard;

        public ICard ExtraCard
        {
            get { return extraCard; }
            set { extraCard = value; }
        }

    }
}

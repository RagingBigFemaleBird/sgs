using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SgsCore.RoleGame
{
    public enum Role
    {
        Unknown,
        Ruler,
        Rebel,
        Loyalist,
        Defector
    }

    class RoleGame : Game
    {
        public class RoleGameRuleTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, object eventArgs)
            {
                Game game = Game.CurrentGame;
                // Deal everyone 4 cards
                foreach (Player player in game.Players)
                {
                    game.DrawCards(player, 4);
                }

                while (true)
                {

                }
            }
        }

        public RoleGame()
        {
        }

        protected override void InstallInitalTriggers()
        {
            GameEvent gameStart = new GameEvent();
            gameStart.EventName = GameEvent.EventType.GameStart;
            RegisterTrigger(gameStart, new RoleGameRuleTrigger());
        }

        public override void Run()
        {
            base.Run();
            
            // Put the whole deck in the dealing deck
            decks[DeckType.Dealing] = cardSet.GetRange(0, cardSet.Count);
        }

    }

    
}

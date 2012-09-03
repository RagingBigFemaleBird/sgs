using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Players;

namespace Sanguosha.Core.Games
{
    public enum Role
    {
        Unknown,
        Ruler,
        Rebel,
        Loyalist,
        Defector
    }

    public class RoleGame : Game
    {
        public class RoleGameRuleTrigger : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                Game game = eventArgs.Game;

                if (game.Players.Count == 0)
                {
                    return;
                }

                // Deal everyone 4 cards
                foreach (Player player in game.Players)
                {
                    game.DrawCards(player, 4);
                }

                game.CurrentPlayer = game.Players.First();
                game.CurrentPhase = TurnPhase.BeforeTurnStart;

                while (true)
                {
                    game.Advance();
                }
            }
        }

        public RoleGame()
        {
        }

        protected override void InitTriggers()
        {
            RegisterTrigger(GameEvent.GameStart, new RoleGameRuleTrigger());
        }
    }

    
}

using Sanguosha.Core.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sanguosha.Core.Utils
{    
    public class GameDelays
    {
        public const int None = 0;
        public const int GameStart = 1000;
        public const int Damage = 400;
        public const int JudgeEnd = 300;
        public const int Discard = 250;
        public const int CardTransfer = 250;
        public const int Draw = 200;
        public const int ChangePhase = 50;
        public const int PlayerAction = 400;
        public const int Awaken = 2550;
        public const int RoleDistribute = 400;
        public const int ServerSideCompensation = 5000;
        public const int UiDelayCompensation = 3000;
        public const int GameBeforeStart = 1200;
        public const int HanBingJian = 2000;
        public const int Imprisoned = 10;


        public static void Delay(int delayInMilliseconds)
        {
            var game = Game.CurrentGame;
            if (game == null || game.IsUiDetached) return;
            int toDelay = delayInMilliseconds;
            if (game.ReplayController != null)
            {
                toDelay = (int)(toDelay / Game.CurrentGame.ReplayController.Speed);
            }
            Thread.Sleep(toDelay);
        }
    }
}

using Sanguosha.Core.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sanguosha.Core.Utils
{
    public enum GameDelayTypes
    {
        GameStart = 1,
        JunWei,
        TieSuoDamage,
        JudgeEnd,
        Discard,
        CardTransfer,
        Draw,
        ChangePhase,
        PlayerAction,
        Awaken,
        RoleDistribute,
        BaGuaZhen,
    }
    public class GameDelays
    {
        private static Dictionary<GameDelayTypes, int> _delays = new Dictionary<GameDelayTypes,int>();

        static GameDelays()
        {
            _delays[GameDelayTypes.GameStart] = 1000;
            _delays[GameDelayTypes.JunWei] = 380;
            _delays[GameDelayTypes.TieSuoDamage] = 610;
            _delays[GameDelayTypes.JudgeEnd] = 500;
            _delays[GameDelayTypes.Discard] = 480;
            _delays[GameDelayTypes.CardTransfer] = 700;
            _delays[GameDelayTypes.Draw] = 400;
            _delays[GameDelayTypes.ChangePhase] = 50;
            _delays[GameDelayTypes.PlayerAction] = 500;
            _delays[GameDelayTypes.Awaken] = 2550;
            _delays[GameDelayTypes.RoleDistribute] = 400;
            _delays[GameDelayTypes.BaGuaZhen] = 2110;
        }

        public static void Delay(GameDelayTypes DelayCategory)
        {
            int toDelay = 200;
            if (_delays.Keys.Contains(DelayCategory)) toDelay = _delays[DelayCategory];
            if (Game.CurrentGame.ReplayController != null) toDelay = (int)(toDelay / Game.CurrentGame.ReplayController.Speed);
            Thread.Sleep(toDelay);
        }
    }
}

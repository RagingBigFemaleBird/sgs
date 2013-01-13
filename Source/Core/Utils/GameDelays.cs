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
        ChangePlayer,
        PlayerAction,
        XinZhan,
        RoleDistribute,
    }
    public class GameDelays
    {
        public static void Delay(GameDelayTypes DelayCategory)
        {
            int toDelay = 200;
            if (DelayCategory == GameDelayTypes.GameStart) toDelay = 1000;
            if (DelayCategory == GameDelayTypes.JunWei) toDelay = 380;
            if (DelayCategory == GameDelayTypes.TieSuoDamage) toDelay = 610;
            if (DelayCategory == GameDelayTypes.JudgeEnd) toDelay = 500;
            if (DelayCategory == GameDelayTypes.Discard) toDelay = 480;
            if (DelayCategory == GameDelayTypes.CardTransfer) toDelay = 700;
            if (DelayCategory == GameDelayTypes.Draw) toDelay = 400;
            if (DelayCategory == GameDelayTypes.ChangePlayer) toDelay = 300;
            if (DelayCategory == GameDelayTypes.PlayerAction) toDelay = 500;
            if (DelayCategory == GameDelayTypes.XinZhan) toDelay = 2550;
            if (DelayCategory == GameDelayTypes.RoleDistribute) toDelay = 400;
            if (Game.CurrentGame.ReplayController != null) toDelay = (int)(toDelay / Game.CurrentGame.ReplayController.Speed);
            Thread.Sleep(toDelay);
        }
    }
}

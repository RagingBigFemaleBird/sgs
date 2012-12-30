using Sanguosha.Core.Games;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Sanguosha.Core.Network;
using Sanguosha.Core.Players;
using Sanguosha.Core.UI;

namespace Sanguosha.Lobby.Server
{
    public class GameService
    {
        public static void StartGameService()
        {
            int totalNumberOfPlayers = 3;
            int timeOutSeconds = 25;
            Trace.Listeners.Clear();

            TextWriterTraceListener twtl = new TextWriterTraceListener(Path.Combine(Directory.GetCurrentDirectory(), AppDomain.CurrentDomain.FriendlyName + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + ".txt"));
            twtl.Name = "TextLogger";
            twtl.TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime;

            ConsoleTraceListener ctl = new ConsoleTraceListener(false);
            ctl.TraceOutputOptions = TraceOptions.DateTime;

            Trace.Listeners.Add(twtl);
            Trace.Listeners.Add(ctl);
            Trace.AutoFlush = true;

            Trace.WriteLine("Log starting");
            Trace.Listeners.Add(new ConsoleTraceListener());
            Game game = new RoleGame(1);
            Sanguosha.Core.Network.Server server;
            server = new Sanguosha.Core.Network.Server(game, totalNumberOfPlayers);
            for (int i = 0; i < totalNumberOfPlayers; i++)
            {
                var player = new Player();
                player.Id = i;
                game.Players.Add(player);
                IUiProxy proxy;
                proxy = new ServerNetworkUiProxy(server, i);
                proxy.TimeOutSeconds = timeOutSeconds;
                proxy.HostPlayer = player;
                game.UiProxies.Add(player, proxy);
            }
            GlobalServerUiProxy pxy = new GlobalServerUiProxy(game, game.UiProxies);
            pxy.TimeOutSeconds = timeOutSeconds;
            game.GlobalProxy = pxy;
            game.NotificationProxy = new DummyNotificationProxy();
            foreach (var g in GameEngine.Expansions.Values)
            {
                game.LoadExpansion(g);
            }
            game.GameServer = server;
            game.Run();
        }

    }
}

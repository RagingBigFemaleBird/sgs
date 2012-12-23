using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.UI;
using Sanguosha.Core.Network;
using System.IO;

namespace Sanguosha
{
    class Program
    {
        static int totalNumberOfPlayers = 3;
        static int timeOutSeconds = 25;
        static void Main(string[] args)
        {
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
            Server server;
            server = new Server(game, totalNumberOfPlayers);
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
            GameEngine.LoadExpansions("./");
            foreach (var g in GameEngine.Expansions.Values)
            {
                game.LoadExpansion(g);
            }
            game.GameServer = server;
            game.Run();
        }
    }
}

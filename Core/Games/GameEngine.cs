using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using Sanguosha.Core.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.UI;

namespace Sanguosha.Core.Games
{
    public class GameEngine
    {
        static GameEngine()
        {
        }

        static Dictionary<string, Expansion> expansions;

        public static Dictionary<string, Expansion> Expansions
        {
            get { return GameEngine.expansions; }
            set { GameEngine.expansions = value; }
        }

        public static void LoadExpansions(string folderPath)
        {
            var files = Directory.GetFiles(folderPath);
            foreach (var file in files)
            {
                Assembly assembly = Assembly.LoadFile(folderPath);
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(typeof(Expansion)))
                    {
                        var exp = Activator.CreateInstance(type) as Expansion;
                        if (exp != null)
                        {
                            expansions.Add(type.Name, exp);
                        }
                    }
                }
            }
        }
    }
}

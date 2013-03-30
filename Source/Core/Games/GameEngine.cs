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
            expansions = new Dictionary<string, Expansion>();
            cardSet = new List<Card>();            
        }

        static Dictionary<string, Expansion> expansions;

        static IList<Card> cardSet;

        public static IList<Card> CardSet
        {
            get { return GameEngine.cardSet; }
            set { GameEngine.cardSet = value; }
        }

        public static Dictionary<string, Expansion> Expansions
        {
            get { return GameEngine.expansions; }
            set { GameEngine.expansions = value; }
        }

        public static int Serialize(CardHandler handler)
        {
            return idOfCardHandler[handler.Name];
        }

        public static CardHandler DeserializeCardHandler(int id)
        {
            return cardHandlers[id].Clone() as CardHandler;
        }

        // Used to serialize/deserialize card handlers.
        private static Dictionary<int, CardHandler> cardHandlers = new Dictionary<int, CardHandler>();
        private static Dictionary<string, int> idOfCardHandler = new Dictionary<string, int>();

        public static void LoadExpansion(string name, Expansion expansion)
        {
            int newId = cardHandlers.Count;
            expansions.Add(name, expansion);
            foreach (var card in expansion.CardSet)
            {
                card.Id = cardSet.Count;
                cardSet.Add(card);
                string typeName = card.Type.Name;
                if (!idOfCardHandler.ContainsKey(typeName))
                {
                    idOfCardHandler.Add(typeName, newId);
                    cardHandlers.Add(newId, card.Type);
                    newId++;
                }
            }
        }

        public static void LoadExpansions(string folderPath)
        {
            Trace.TraceInformation("LOADING CARDSETS FROM : " + folderPath);
            var files = (from f in Directory.GetFiles(folderPath) where f.EndsWith(".dll") select f).OrderBy(
                        (a) => { int idx = Properties.Settings.Default.LoadSequence.IndexOf(Path.GetFileNameWithoutExtension(a).ToLower()); if (idx < 0) return int.MaxValue; return idx; });
            foreach (var file in files)
            {

                try
                {
                    Assembly assembly = Assembly.LoadFile(Path.GetFullPath(file));
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (type.IsSubclassOf(typeof(Expansion)))
                        {
                            var exp = Activator.CreateInstance(type) as Expansion;
                            if (exp != null)
                            {
                                if (expansions.ContainsKey(type.FullName))
                                {
                                    if (!expansions.ContainsValue(exp))
                                    {
                                        Trace.TraceWarning("Cannot load two different expansions with same name: {0}.", type.FullName);
                                    }
                                }
                                else
                                {
                                    LoadExpansion(type.FullName, exp);
                                }
                            }
                        }
                    }
                }
                catch (BadImageFormatException)
                {
                    continue;
                }
            }
        }
    }
}

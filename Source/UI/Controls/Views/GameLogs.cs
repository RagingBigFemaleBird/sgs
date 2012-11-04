using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Players;
using System.Windows.Documents;
using Sanguosha.Core.UI;
using Sanguosha.Core.Games;
using Sanguosha.Core.Cards;
using System.Diagnostics;

namespace Sanguosha.UI.Controls
{
    public class GameLogs
    {
        public GameLogs()
        {
            Logs = new Dictionary<Player, FlowDocument>();
            GlobalLog = new FlowDocument();
        }

        public FlowDocument GlobalLog
        {
            get;
            set;
        }

        private IDictionary<Player, FlowDocument> logs;

        public IDictionary<Player, FlowDocument> Logs
        {
            get { return logs; }
            private set { logs = value; }
        }

        public void AppendLog(ActionLog log)
        {
            var docs = (from pair in Logs
                        where (log.Targets.Contains(pair.Key) || log.Source == pair.Key)
                        select pair.Value).Concat(new List<FlowDocument>() { GlobalLog });
            
            var paragraph = LogFormatter.RichTranslateMainLog(log);
            if (paragraph.Inlines.Count > 0)
            {
                foreach (var doc in docs)
                {
                    doc.Blocks.Add(paragraph);
                }
            }
        }

        public void AppendCardMoveLog(List<Card> cards, DeckPlace source, DeckPlace dest)
        {
            if (source.Player == null && dest.Player == null || cards.Count == 0)
            {
                return;
            }
            var reasons = (from card in cards select card.Log.GameAction).Distinct();
            Trace.Assert(reasons.Count() == 1);
            var reason = reasons.First();

            var docs = (from pair in Logs
                        where (source.Player == pair.Key || dest.Player == pair.Key)
                        select pair.Value).Concat(new List<FlowDocument>() { GlobalLog });
            var paragraph = LogFormatter.RichTranslateCardMove(cards, source, dest, reason);
            if (paragraph.Inlines.Count > 0)
            {
                foreach (var doc in docs)
                {
                    doc.Blocks.Add(paragraph);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Players;
using System.Windows.Documents;
using Sanguosha.Core.UI;

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
            
            foreach (var doc in docs)
            {
                var paragraph = LogFormatter.RichTranslateMainLog(log);
                doc.Blocks.Add(paragraph);
            }
        }
    }
}

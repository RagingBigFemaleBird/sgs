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
using System.Windows.Shapes;
using System.Windows.Media;

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
                if (paragraph != null && paragraph.Inlines.Count > 0)
                {
                    doc.Blocks.Add(paragraph);
                }
            }
        }

        public void AppendPickHeroLog(Player player, bool isPrimaryHero)
        {
            if (!Logs.ContainsKey(player)) return;
            
            if (isPrimaryHero && player.Hero == null) return;
            else if (!isPrimaryHero && player.Hero2 == null) return;

            List<FlowDocument> docs = new List<FlowDocument>() { Logs[player], GlobalLog };
            foreach (var doc in docs)
            {
                Paragraph para = LogFormatter.RichTranslatePickHero(player, isPrimaryHero);
                if (para != null)
                {
                    doc.Blocks.Add(para);
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

            foreach (var doc in docs)
            {
                var paragraph = LogFormatter.RichTranslateCardMove(cards, source, dest, reason);

                if (paragraph.Inlines.Count > 0)
                {
                    doc.Blocks.Add(paragraph);
                }
            }
        }
        private static string _separatorMagic = "a432kdfsad9f134";

        public void AppendSeparator()
        {
            var docs = Logs.Values.Concat(new List<FlowDocument>() { GlobalLog });
            foreach (var doc in docs)
            {
                if (doc.Blocks.Count() != 0 && doc.Blocks.Last().Name == _separatorMagic)
                {
                    continue;
                }

                var para = new Paragraph();
                para.Name = _separatorMagic;
                var rect1 = new Rectangle();
                rect1.Width = 210;
                rect1.Height = 1;
                rect1.Fill = new SolidColorBrush(Colors.Black);

                var rect2 = new Rectangle();
                rect2.Width = 210;
                rect2.Height = 1;
                rect2.Fill = new SolidColorBrush(new Color() { R = 77, G = 74, B = 66, A = 255 });

                para.Inlines.Add(rect1);
                para.Inlines.Add(rect2);

                doc.Blocks.Add(para);
            }
        }

        public void AppendDamageLog(Player source, Player target, int magnitude, DamageElement element)
        {
            if (!Logs.ContainsKey(target)) return;
            Trace.Assert(target != null);
            List<FlowDocument> docs = new List<FlowDocument>() { Logs[target], GlobalLog };
            if (source != null) docs.Add(Logs[source]);
            foreach (var doc in docs)
            {
                Paragraph para = LogFormatter.RichTranslateDamage(source, target, magnitude, element);
                if (para != null)
                {
                    doc.Blocks.Add(para);
                }
            }
        }

        public void AppendDeathLog(Player p, Player by)
        {
            if (!Logs.ContainsKey(p)) return;
            List<FlowDocument> docs = new List<FlowDocument>() { Logs[p], GlobalLog };
            if (by != null) docs.Add(Logs[by]);
            foreach (var doc in docs)
            {
                Paragraph para = LogFormatter.RichTranslateDeath(p, by);
                if (para != null)
                {
                    doc.Blocks.Add(para);
                }
                para = LogFormatter.RichTranslateRole(p);
                if (para != null && para.Inlines.Count > 0)
                {
                    doc.Blocks.Add(para);
                }
            }
        }

        internal void AppendMultipleChoiceLog(Player p, string answer)
        {
            if (!Logs.ContainsKey(p)) return;
            List<FlowDocument> docs = new List<FlowDocument>() { Logs[p], GlobalLog };
            foreach (var doc in docs)
            {
                Paragraph para = LogFormatter.RichTranslateChoice(p, answer);
                if (para != null && para.Inlines.Count > 0)
                {
                    doc.Blocks.Add(para);
                }
                else
                {
                    break;
                }
            }
        }

        internal void AppendLoseHealthLog(Player player, int delta)
        {
            if (!Logs.ContainsKey(player)) return;
            List<FlowDocument> docs = new List<FlowDocument>() { Logs[player], GlobalLog };
            foreach (var doc in docs)
            {
                Paragraph para = LogFormatter.RichTranslateLoseHealth(player, delta);
                if (para != null)
                {
                    doc.Blocks.Add(para);
                }
            }
        }

        internal void AppendShowCardsLog(Player p, IList<Card> cards)
        {
            if (!Logs.ContainsKey(p)) return;
            List<FlowDocument> docs = new List<FlowDocument>() { Logs[p], GlobalLog };
            foreach (var doc in docs)
            {
                Paragraph para = LogFormatter.RichTranslateShowCards(p, cards);
                if (para != null)
                {
                    doc.Blocks.Add(para);
                }
            }
        }

        internal void AppendReforgeLog(Player p, ICard card)
        {
            if (!Logs.ContainsKey(p)) return;
            List<FlowDocument> docs = new List<FlowDocument>() { Logs[p], GlobalLog };
            foreach (var doc in docs)
            {
                Paragraph para = LogFormatter.RichTranslateReforgeCard(p, card);
                if (para != null)
                {
                    doc.Blocks.Add(para);
                }
            }
        }

        public void AppendLogEvent(List<Player> players, Prompt custom, bool useUICard = true)
        {
            var docs = (from pair in Logs
                        where players.Contains(pair.Key)
                        select pair.Value).Concat(new List<FlowDocument>() { GlobalLog });

            foreach (var doc in docs)
            {
                Paragraph para = new Paragraph();
                para.Inlines.AddRange(LogFormatter.TranslateLogEvent(custom, useUICard));
                if (para != null)
                {
                    doc.Blocks.Add(para);
                }
            }
        }

        internal void AppendJudgeResultLog(Player p, Card card, ActionLog log, bool? isSuccess, bool isFinalResult)
        {
            List<FlowDocument> docs = new List<FlowDocument>() { Logs[p], GlobalLog };
            foreach (var doc in docs)
            {
                Paragraph para = LogFormatter.RichTranslateJudgeResult(p, card, log, isFinalResult);

                if (para != null)
                {
                    doc.Blocks.Add(para);
                }
                if (isSuccess != null)
                {
                    para = LogFormatter.RichTranslateJudgeResultEffectiveness(p, log, isSuccess == true);

                    if (para != null)
                    {
                        doc.Blocks.Add(para);
                    }
                }
            }
        }

        internal void AppendRecoverHealthLog(Player player, int delta)
        {
            List<FlowDocument> docs = new List<FlowDocument>() { Logs[player], GlobalLog };
            foreach (var doc in docs)
            {
                Paragraph para = LogFormatter.RichTranslateRecoverHealth(player, delta);
                if (para != null)
                {
                    doc.Blocks.Add(para);
                }
            }
        }
    }
}

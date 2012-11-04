using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Players;
using System.Windows;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using System.Diagnostics;
using System.Windows.Documents;
using Sanguosha.Core.Cards;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sanguosha.UI.Controls
{
    public class LogFormatter
    {
        public static string Translate(Player player)
        {
            if (player == null) return string.Empty;
            string key = string.Format("Hero.{0}.Name", player.Hero.Name);
            string name = Application.Current.TryFindResource(key) as string;
            if (name == null) return string.Empty;
            return name;
        }

        public static string Translate(ISkill skill)
        {
            if (skill == null) return string.Empty;
            string key = string.Format("Skill.{0}.Name", skill.GetType().Name);
            string name = Application.Current.TryFindResource(key) as string;
            if (name == null) return string.Empty;
            return name;
        }

        public static string TranslateCardFootnote(ActionLog log)
        {
            string source = Translate(log.Source);
            string dest = string.Empty;
            Trace.Assert(log.Targets != null);
            if (log.Targets != null && log.Targets.Count == 1)
            {
                dest = "对" + Translate(log.Targets[0]);
            }
            string skill = Translate(log.SkillAction);
            string formatter = source;
            switch(log.GameAction)
            {
                case GameAction.None:
                    return formatter;
                case GameAction.Use:
                    return string.Format("{0}{1}{2}", source, dest, skill);
                case GameAction.Play:
                    return string.Format("{0}{2}{1}打出", source, dest, skill);
                case GameAction.PlaceIntoDiscard:
                    return string.Format("{0}{1}置入弃牌堆", source, skill);
                case GameAction.Discard:
                    return string.Format("{0}{1}弃置", source, skill);
            }
            return string.Empty;
        }

        public static IList<Inline> RichTranslate(Card c)
        {
            IList<Inline> list = new List<Inline>();
            if (c.Id < 0) return list;
            CardViewModel card = new CardViewModel() { Card = c };
            string typeString = Application.Current.TryFindResource(string.Format("Card.{0}.Name", card.TypeString)) as string;
            if (typeString != null)
            {
                list.Add(new Run("【" + typeString) { Foreground = new SolidColorBrush(Colors.Yellow) });
                Run run = new Run();
                run.Foreground = Application.Current.Resources[string.Format("Card.Suit.{0}.SuitBrush", card.Suit)] as Brush;
                run.Text = string.Format("{0}{1}", Application.Current.Resources[string.Format("Card.Suit.{0}.SuitText", card.Suit)], card.RankString);
                list.Add(run);
                list.Add(new Run("】") { Foreground = new SolidColorBrush(Colors.Yellow) });
            }
            return list;
        }

        public static IList<Inline> RichTranslate(IList<Card> cards)
        {
            List<Inline> list = new List<Inline>();
            if (cards.Count == 0) return list;
            else if (cards.Count > 1) list.Add(new Run("{0}张"));
            list.Add(new Run("卡牌"));
            foreach (var card in cards)
            {
                list.AddRange(RichTranslate(card));
            }
            return list;
        }

        public static IList<Inline> RichTranslate(CardViewModel card)
        {
            IList<Inline> list = new List<Inline>();
            string typeString = Application.Current.TryFindResource(string.Format("Card.{0}.Name", card.TypeString)) as string;
            if (typeString != null)
            {
                list.Add(new Run("【" + typeString) { Foreground = new SolidColorBrush(Colors.Yellow) });
                Run run = new Run();
                run.Foreground = Application.Current.Resources[string.Format("Card.Suit.{0}.SuitBrush", card.Suit)] as Brush;
                run.Text = string.Format("{0}{1}", Application.Current.Resources[string.Format("Card.Suit.{0}.SuitText", card.Suit)], card.RankString);
                list.Add(run);
                list.Add(new Run("】") { Foreground = new SolidColorBrush(Colors.Yellow) });
            }
            return list;
        }

        public static IList<Inline> RichTranslate(ISkill skill)
        {
            IList<Inline> list = new List<Inline>();
            string skillstr = string.Format("\"{0}\"", Translate(skill));
            list.Add(new Run(skillstr) { Foreground = new SolidColorBrush(Colors.Yellow) });
            return list;
        }

        public static Block RichTranslateMainLog(ActionLog log)
        {
            Paragraph paragraph = new Paragraph();
            string source = Translate(log.Source);
            string dests = string.Empty;
            Trace.Assert(log.Targets != null);
            if (log.Targets != null && log.Targets.Count > 0)
            {
                StringBuilder builder = new StringBuilder();                
                builder.Append(Translate(log.Targets[0]));
                for (int i = 1; i < log.Targets.Count; i++)
                {
                    builder.Append("，");
                    builder.Append(Translate(log.Targets[1]));
                }
                dests = builder.ToString();
            }
            
            string skill = Translate(log.SkillAction);
            string formatter = source;
            switch (log.GameAction)
            {
                case GameAction.None:
                    if (log.SkillAction != null)
                    {
                        paragraph.Inlines.Add(string.Format("{0}发动了技能", source));
                        paragraph.Inlines.AddRange(RichTranslate(log.SkillAction));
                        if (dests != string.Empty)
                        {
                            paragraph.Inlines.Add("，目标是" + dests);
                        }
                    }
                    break;
                case GameAction.Use:
                    string toDests = (dests == string.Empty) ?  string.Empty : ("对" + dests);
                    paragraph.Inlines.Add(string.Format("{0}{1}使用了", source, toDests));
                    if (log.CardAction is CompositeCard)
                    {
                        // paragraph.Inlines.AddRange(RichTranslate((log.CardAction as CompositeCard).Subcards));
                    }
                    else if (log.CardAction is Card)
                    {
                        paragraph.Inlines.AddRange(RichTranslate(log.CardAction as Card));
                    }
                    else
                    {
                        Trace.Assert(false);
                    }
                    break;
                case GameAction.Play:
                    // return string.Format("{0}{2}{1}打出", source, dest, skill);
                    break;
                case GameAction.PlaceIntoDiscard:
                    // return string.Format("{0}{1}置入弃牌堆", source, skill);
                    break;
                case GameAction.Discard:
                    //return string.Format("{0}{1}弃置", source, skill);
                    break;
            }
            return paragraph;
        }
    }
}

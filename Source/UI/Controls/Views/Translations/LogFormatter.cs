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
using Sanguosha.Core.Games;
using Sanguosha.Core.Heroes;
using System.Windows.Media.Imaging;

namespace Sanguosha.UI.Controls
{
    public class LogFormatter
    {
        public static string Translate(Hero hero)
        {
            string key = string.Format("Hero.{0}.Name", hero.Name);
            string name = Application.Current.TryFindResource(key) as string;
            if (name == null) return string.Empty;
            return name;
        }

        public static string Translate(Player player)
        {
            if (player == null) return string.Empty;
            if (player.Hero == null)
            {
                return Game.CurrentGame.Settings.Accounts[player.Id].UserName ?? string.Empty;
            }
            return Translate(player.Hero);
        }

        public static string Translate(ISkill skill)
        {
            if (skill == null) return string.Empty;
            string key = string.Format("Skill.{0}.Name", skill.GetType().Name);
            string name = Application.Current.TryFindResource(key) as string;
            if (name == null) return string.Empty;
            return name;
        }

        public static string Translate(Card card)
        {
            if (card == null) return string.Empty;
            string key = string.Format("Card.{0}.Name", card.Type.GetType().Name);
            string name = Application.Current.TryFindResource(key) as string;
            if (name == null) return string.Empty;
            return name;
        }

        public static List<Inline> Format(string format, List<IList<Inline>> args)
        {
            if (args == null || args.Count == 0) return new List<Inline>() { new Run(format) };

            List<Inline> result = new List<Inline>();
            string separator = "|";
            List<string> separators = new List<string>();

            int i = 0;
            for (i = 0; i < args.Count; i++) separators.Add(separator);
            string temp = string.Format(format, separators.ToArray());

            i = 0;
            foreach (string s in temp.Split(separator.ToCharArray()))
            {
                if (s != string.Empty) result.Add(new Run(s));
                if (i < args.Count) result.AddRange(args[i]);
                i++;
            }
            return result;
        }

        public static IList<Inline> TranslateLogEvent(Prompt custom, bool useUICard = true)
        {
            string format = Application.Current.TryFindResource(custom.ResourceKey) as string;
            if (format == null)
            {
                return new List<Inline>() { new Run(custom.ResourceKey) };
            }
            List<IList<Inline>> values = new List<IList<Inline>>();
            foreach (object arg in custom.Values)
            {
                IList<Inline> value = new List<Inline>();
                if (arg is Player) value.Add(new Run(Translate(arg as Player)));
                else if (arg is Card) value = RichTranslate(arg as Card, useUICard);
                else if (arg is ISkill) value = RichTranslate(arg as ISkill);
                else if (arg is CardHandler) value = RichTranslate(arg as CardHandler);
                else if (arg is Prompt) value.Add(new Run(PromptFormatter.Format(arg as Prompt)));
                if (value.Count == 0)
                {
                    value.Add(new Run(arg.ToString()));
                }
                values.Add(value);
            }
            return Format(format, values);
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
            switch (log.GameAction)
            {
                case GameAction.None:
                    return string.Concat(source, skill);
                case GameAction.Use:
                    return string.Concat(source, dest, skill);
                case GameAction.Play:
                    return string.Concat(source, skill, dest, "打出");
                case GameAction.PlaceIntoDiscard:
                    return string.Concat(source, skill, "置入弃牌堆");
                case GameAction.Discard:
                    return string.Concat(source, skill, "弃置");
                case GameAction.Show:
                    return string.Concat(source, "展示");
                case GameAction.Judge:
                    {
                        string card = Translate(log.CardAction as Card);
                        return string.Concat(source, skill, card, "判定");
                    }
                case GameAction.ReplaceJudge:
                    return string.Concat(source, skill, "改判");
                case GameAction.Reforge:
                    return string.Concat(source, skill, "重铸");
            }
            return string.Empty;
        }

        public static IList<Inline> RichTranslate(CardHandler cardType)
        {
            IList<Inline> list = new List<Inline>();
            if (cardType == null) return list;
            string typeString = Application.Current.TryFindResource(string.Format("Card.{0}.Name", cardType.CardType)) as string;
            if (typeString != null)
            {
                list.Add(new Run(string.Format("【{0}】", typeString)) { Foreground = new SolidColorBrush(Colors.Yellow) });
            }
            return list;
        }

        public static IList<Inline> RichTranslate(Card c, bool useUiCard = true)
        {
            IList<Inline> list = new List<Inline>();
            if (c.Id < 0) return list;
            CardViewModel card = new CardViewModel() { Card = c };
            string typeString = Application.Current.TryFindResource(string.Format("Card.{0}.Name", useUiCard ? card.TypeString : c.Type.CardType)) as string;
            if (typeString != null)
            {
                list.Add(new Run("【" + typeString) { Foreground = new SolidColorBrush(Colors.Yellow) });
                Run run = new Run();
                run.Foreground = Application.Current.Resources[string.Format("Card.Suit.{0}.SuitBrush", useUiCard ? card.Suit : c.Suit)] as Brush;
                run.Text = string.Format("{0}{1}", Application.Current.Resources[string.Format("Card.Suit.{0}.SuitText", useUiCard ? card.Suit : c.Suit)], card.RankString);
                list.Add(run);
                list.Add(new Run("】") { Foreground = new SolidColorBrush(Colors.Yellow) });
            }
            return list;
        }

        public static IList<Inline> RichTranslate(IList<Card> cards)
        {
            List<Inline> list = new List<Inline>();
            if (cards.Count == 0) return list;
            else if (cards.Count > 1 || cards[0].Id < 0)
            {
                list.Add(new Run(string.Format("{0}张卡牌", cards.Count)));
            }

            if (cards.Any(c => c.Id < 0) && cards.Any(c => c.Id > 0))
            {
                list.Add(new Run("，其中有"));
            }

            foreach (var card in cards)
            {
                list.AddRange(RichTranslate(card));
            }
            return list;
        }

        private static string TranslateHeroName(Hero hero)
        {
            return Application.Current.TryFindResource(string.Format("Hero.{0}.Name", hero.Name)) as string;
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

        public static IList<Inline> RichTranslate(ICard card, bool useUiCard = true)
        {
            List<Inline> list = new List<Inline>();
            if (card is Card)
            {
                list.AddRange(RichTranslate(card as Card, useUiCard));
            }
            else
            {
                list.AddRange(RichTranslate(card.Type));
            }
            return list;
        }

        public static IList<Inline> RichTranslate(ISkill skill)
        {
            IList<Inline> list = new List<Inline>();
            string skillstr = string.Format("\"{0}\"", Translate(skill));
            list.Add(new Run(skillstr) { Foreground = YellowBrush });
            return list;
        }

        public static string Translate(IList<Player> players)
        {
            if (players != null && players.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(Translate(players[0]));
                for (int i = 1; i < players.Count; i++)
                {
                    builder.Append("，");
                    builder.Append(Translate(players[i]));
                }
                return builder.ToString();
            }
            return string.Empty;
        }

        public static Paragraph RichTranslateMainLog(ActionLog log)
        {
            Paragraph paragraph = new Paragraph();
            string source = Translate(log.Source);
            string dests = Translate(log.Targets);
            string secDests = Translate(log.SecondaryTargets);

            IList<Inline> skillInline = RichTranslate(log.SkillAction);
            string formatter = source;
            switch (log.GameAction)
            {
                case GameAction.None:
                    if (log.SkillAction != null)
                    {
                        ISkill skill = log.SkillAction;
                        if (skill is ActiveSkill)
                        {
                            IEquipmentSkill equipSkill = log.SkillAction as IEquipmentSkill;
                            if (equipSkill != null && equipSkill.ParentEquipment is Weapon)
                            {
                                paragraph.Inlines.Add(string.Format("{0}{1}发动了", source, dests));
                                paragraph.Inlines.AddRange(RichTranslate(equipSkill.ParentEquipment));
                                paragraph.Inlines.Add("的武器特效");
                            }
                            else
                            {
                                paragraph.Inlines.Add(string.Format("{0}发动了技能", source));
                                paragraph.Inlines.AddRange(RichTranslate(log.SkillAction));
                                if (dests != string.Empty)
                                {
                                    paragraph.Inlines.Add("，目标是" + dests);
                                }
                                if (secDests != string.Empty)
                                {
                                    paragraph.Inlines.Add("，" + secDests);
                                }
                            }
                        }
                        else if (skill is TriggerSkill)
                        {
                            if (skill is IEquipmentSkill)
                            {
                                IEquipmentSkill equipSkill = log.SkillAction as IEquipmentSkill;
                                if (equipSkill.ParentEquipment is Weapon)
                                {
                                    paragraph.Inlines.Add(string.Format("{0}发动了武器特效", source));
                                    paragraph.Inlines.AddRange(RichTranslate(equipSkill.ParentEquipment));
                                }
                                else if (equipSkill.ParentEquipment is Armor)
                                {
                                    paragraph.Inlines.Add(string.Format("{0}的", source));
                                    paragraph.Inlines.AddRange(RichTranslate(equipSkill.ParentEquipment));
                                    paragraph.Inlines.Add(string.Format("效果被触发", source));
                                }
                            }
                            else
                            {
                                paragraph.Inlines.Add(string.Format("{0}的武将技能", source));
                                paragraph.Inlines.AddRange(RichTranslate(log.SkillAction));
                                paragraph.Inlines.Add(string.Format("被触发"));
                            }
                            if (dests != string.Empty)
                            {
                                paragraph.Inlines.Add("，目标是" + dests);
                            }
                        }
                        else if (skill is CardTransformSkill)
                        {
                            CompositeCard card = log.CardAction as CompositeCard;
                            Trace.Assert(card != null);
                            paragraph.Inlines.Add(string.Format("{0}发动了技能", source));
                            paragraph.Inlines.AddRange(RichTranslate(log.SkillAction));
                            if (card.Subcards.Count > 0)
                            {
                                paragraph.Inlines.Add("，将");
                                paragraph.Inlines.AddRange(RichTranslate(card.Subcards));
                                paragraph.Inlines.Add("当作一张");
                                paragraph.Inlines.AddRange(RichTranslate(card.Type));
                            }
                            if (dests != string.Empty) paragraph.Inlines.Add(string.Format("，目标是" + dests));
                        }
                    }
                    break;
                case GameAction.Use:
                    string toDests = (dests == string.Empty) ? string.Empty : ("对" + dests);
                    paragraph.Inlines.Add(string.Format("{0}{1}使用了", source, toDests));
                    paragraph.Inlines.AddRange(RichTranslate(log.CardAction, false));
                    if (secDests != string.Empty) paragraph.Inlines.Add(string.Format("目标是" + secDests));
                    break;
                case GameAction.Play:
                    paragraph.Inlines.Add(string.Format("{0}打出了", source));
                    paragraph.Inlines.AddRange(RichTranslate(log.CardAction, false));
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

        public static Paragraph RichTranslateKeyLog(ActionLog log)
        {
            Paragraph paragraph = new Paragraph();
            string source = Translate(log.Source);
            string dests = Translate(log.Targets);

            IList<Inline> skillInline = RichTranslate(log.SkillAction);
            string formatter = source;
            switch (log.GameAction)
            {
                case GameAction.None:
                    if (log.SkillAction != null)
                    {
                        ISkill skill = log.SkillAction;
                        if (skill is ActiveSkill)
                        {
                            if (!string.IsNullOrEmpty(dests))
                            {
                                dests = "对" + dests;
                            }
                            if (log.SkillAction is IEquipmentSkill)
                            {
                                IEquipmentSkill equipSkill = log.SkillAction as IEquipmentSkill;
                                if (equipSkill.ParentEquipment is Weapon)
                                {
                                    paragraph.Inlines.Add(string.Format("{0}{1}发动了", source, dests));
                                    paragraph.Inlines.AddRange(RichTranslate(equipSkill.ParentEquipment));
                                    paragraph.Inlines.Add("的武器特效");
                                }
                            }
                            else
                            {
                                paragraph.Inlines.Add(string.Format("{0}{1}发动了武将技能", source, dests));
                                paragraph.Inlines.AddRange(RichTranslate(log.SkillAction));
                            }
                        }
                        else if (skill is TriggerSkill)
                        {
                            if (skill is IEquipmentSkill)
                            {
                                IEquipmentSkill equipSkill = log.SkillAction as IEquipmentSkill;
                                if (equipSkill.ParentEquipment is Weapon)
                                {
                                    paragraph.Inlines.Add(string.Format("{0}发动了武器特效", source));
                                    paragraph.Inlines.AddRange(RichTranslate(equipSkill.ParentEquipment));
                                }
                                else if (equipSkill.ParentEquipment is Armor)
                                {
                                    paragraph.Inlines.Add(string.Format("{0}的", source));
                                    paragraph.Inlines.AddRange(RichTranslate(equipSkill.ParentEquipment));
                                    paragraph.Inlines.Add(string.Format("效果被触发", source));
                                }
                            }
                            else
                            {
                                paragraph.Inlines.Add(string.Format("{0}的武将技能", source));
                                paragraph.Inlines.AddRange(RichTranslate(log.SkillAction));
                                paragraph.Inlines.Add(string.Format("被触发"));
                            }
                        }
                    }
                    break;
                case GameAction.Use:
                    if (log.CardAction.GetType().Name == "WuXieKeJi" && log.SkillAction == null)
                    {
                        paragraph.Inlines.Add(string.Format("{0}使用了卡牌", source));
                        paragraph.Inlines.AddRange(RichTranslate(log.CardAction));
                    }
                    break;
                case GameAction.PlaceIntoDiscard:
                    // return string.Format("{0}{1}置入弃牌堆", source, skill);
                    break;
                case GameAction.Discard:
                    //return string.Format("{0}{1}弃置", source, skill);
                    break;
            }
            paragraph.TextAlignment = TextAlignment.Center;
            return paragraph;
        }

        public static Paragraph RichTranslateCardMove(List<Card> cards, DeckPlace source, DeckPlace dest, GameAction reason)
        {
            string sourceStr = Translate(source.Player);
            string destStr = Translate(dest.Player);
            var cardsInline = RichTranslate(cards);
            Paragraph paragraph = new Paragraph();
            if (source.Player != null)
            {
                if (reason == GameAction.Discard)
                {
                    paragraph.Inlines.Add(string.Format("{0}弃置了", sourceStr));
                    paragraph.Inlines.AddRange(cardsInline);
                }
                else if (reason == GameAction.PlaceIntoDiscard)
                {
                    paragraph.Inlines.Add(string.Format("{0}将", sourceStr));
                    paragraph.Inlines.AddRange(cardsInline);
                    paragraph.Inlines.Add("置入了弃牌堆");
                }
            }

            if (dest.Player != null)
            {
                bool added = true;
                if (source.DeckType == DeckType.Dealing && dest.DeckType == DeckType.Hand)
                {
                    paragraph.Inlines.Add(string.Format("{0}从牌堆里摸了", destStr));
                }
                else if (source.DeckType == DeckType.Discard)
                {
                    paragraph.Inlines.Add(string.Format("{0}从弃牌堆里回收了", destStr));
                }
                else if (source.Player == dest.Player)
                {
                    if (dest.DeckType == DeckType.Equipment)
                    {
                        paragraph.Inlines.Add(string.Format("{0}装备了", destStr));
                    }
                    else if (source.DeckType == DeckType.Hand && dest.DeckType == DeckType.Hand)
                    {
                        paragraph.Inlines.Add(string.Format("{0}获得了自己的", destStr));
                    }
                    else if (dest.DeckType is PrivateDeckType)
                    {
                        paragraph.Inlines.Add(string.Format("{0}将", destStr));
                        paragraph.Inlines.AddRange(cardsInline);
                        paragraph.Inlines.Add(string.Format("置于武将牌上"));
                        added = false;
                    }
                    else if (dest.DeckType == DeckType.Hand)
                    {
                        paragraph.Inlines.Add(string.Format("{0}获得了", destStr));
                    }
                    else added = false;
                }
                else if (dest.DeckType == DeckType.Hand || dest.DeckType == DeckType.Equipment)
                {
                    var owners = (from card in cards select card.Owner).Distinct();
                    if (owners.Contains(null))
                    {
                        Trace.TraceWarning("Cannot resolve log: reason is {0}, from {1}{2} to {3}{4}", reason, Translate(source.Player), source.DeckType, Translate(dest.Player), dest.DeckType);
                        paragraph.Inlines.Add(string.Format("{0}获得了", destStr));
                    }
                    else
                    {
                        List<Player> players = new List<Player>(owners);
                        paragraph.Inlines.Add(string.Format("{0}获得了{1}的", destStr, Translate(players)));
                        if (dest.DeckType == DeckType.Equipment)
                        {
                            paragraph.Inlines.AddRange(cardsInline);
                            paragraph.Inlines.Add(string.Format("并装备上"));
                            added = false;
                        }
                    }
                }
                else
                {
                    added = false;
                }
                if (added)
                {
                    paragraph.Inlines.AddRange(cardsInline);
                }
            }
            return paragraph;
        }

        public static Paragraph RichTranslatePickHero(Player player, bool isPrimaryHero)
        {
            Paragraph para = new Paragraph();
            var hero = isPrimaryHero ? player.Hero : player.Hero2;
            string name;
            if (player.Role == Role.Ruler)
            {
                name = "主公";
            }
            else name = Translate(player);

            string heroName = TranslateHeroName(hero);
            para.Inlines.Add(new Run(string.Format("{0}选择了", name)) { Foreground = OrangeBrush });
            para.Inlines.Add(heroName);
            para.Inlines.Add(new Run("作为" + (isPrimaryHero ? "武将" : "副将")) { Foreground = OrangeBrush });
            return para;
        }

        public static string Translate(DamageElement element)
        {
            switch (element)
            {
                case DamageElement.None:
                    return string.Empty;
                case DamageElement.Fire:
                    return "火属性";
                case DamageElement.Lightning:
                    return "雷属性";
            }
            Trace.Assert(false);
            return string.Empty;
        }

        public static Paragraph RichTranslateDamage(Player source, Player target, int magnitude, DamageElement element)
        {
            Paragraph para = new Paragraph();
            para.Inlines.Add(new Run(Translate(target)));
            string sourceStr = Translate(source);
            if (sourceStr != string.Empty)
            {
                sourceStr += "造成的";
            }
            string damageStr = string.Format("受到{0}{1}点{2}伤害，体力为{3}", sourceStr, magnitude, Translate(element), target.Health);

            para.Inlines.Add(new Run(damageStr) { Foreground = RedBrush });
            return para;
        }

        static Brush RedBrush = new SolidColorBrush(new Color() { R = 204, G = 0, B = 0, A = 255 });
        static Brush OrangeBrush = new SolidColorBrush(new Color() { R = 255, G = 102, B = 0, A = 255 });
        static Brush YellowBrush = new SolidColorBrush(Colors.Yellow);
        static Brush GreenBrush = new SolidColorBrush(new Color() { R = 0, G = 204, B = 0, A = 255 });
        static Brush DarkGreenBrush = new SolidColorBrush(new Color() { R = 104, G = 135, B = 41, A = 255 });

        public static Paragraph RichTranslateDeath(Player p, Player by)
        {
            Paragraph para = new Paragraph();
            string deadPerson = Translate(p);
            if (by == p)
            {
                para.Inlines.Add(deadPerson);
                para.Inlines.Add(new Run("自杀") { Foreground = RedBrush });
                return para;
            }
            else if (by != null)
            {
                para.Inlines.Add(Translate(by));
                para.Inlines.Add(new Run("杀死了") { Foreground = RedBrush });
                para.Inlines.Add(deadPerson);
                para.Inlines.Add(new Run("，") { Foreground = RedBrush });
            }
            para.Inlines.Add(deadPerson);
            para.Inlines.Add(new Run("阵亡") { Foreground = RedBrush });
            return para;
        }

        public static Paragraph RichTranslateRole(Player p)
        {
            Paragraph para = new Paragraph();
            string roleStr = Translate(p.Role);
            if (roleStr == string.Empty) return null;
            para.Inlines.Add(string.Format("{0}的身份是{1}", Translate(p), roleStr));
            return para;
        }

        public static string Translate(Role role)
        {
            string key = string.Format("Role.{0}.Name", role);
            string name = Application.Current.TryFindResource(key) as string;
            if (name == null) return string.Empty;
            return name;
        }

        public static Paragraph RichTranslateChoice(Player p, string answer)
        {
            Paragraph para = new Paragraph();
            string name = Application.Current.TryFindResource(answer) as string;
            if (name == null || name == string.Empty) return null;
            para.Inlines.Add(string.Format("{0}选择了", Translate(p)));
            para.Inlines.Add(new Run(string.Format("“{0}”", name)) { Foreground = YellowBrush });
            return para;
        }

        internal static Paragraph RichTranslateLoseHealth(Player player, int delta)
        {
            Paragraph para = new Paragraph();
            para.Inlines.Add(new Run(Translate(player)));
            string damageStr = string.Format("失去了{0}点体力，体力值为{1}", delta, player.Health);

            para.Inlines.Add(new Run(damageStr) { Foreground = RedBrush });
            return para;
        }

        internal static Paragraph RichTranslateShowCards(Player player, IList<Card> cards)
        {
            Paragraph para = new Paragraph();
            para.Inlines.Add(Translate(player));
            para.Inlines.Add(cards.Count == 1 ? "展示了一张手牌" : "展示了手牌");
            para.Inlines.AddRange(RichTranslate(cards));
            return para;
        }

        internal static Paragraph RichTranslateReforgeCard(Player player, ICard card)
        {
            Paragraph para = new Paragraph();
            para.Inlines.Add(Translate(player));
            para.Inlines.Add("重铸了");
            para.Inlines.AddRange(card is CompositeCard ? RichTranslate((card as CompositeCard).Subcards) : RichTranslate(card));
            return para;
        }

        internal static Paragraph RichTranslateJudgeResult(Player p, Card card, ActionLog log, bool isFinalResult)
        {
            Paragraph para = new Paragraph();
            para.Inlines.Add(Translate(p) + "的");
            if (log.CardAction != null)
            {
                para.Inlines.AddRange(RichTranslate(log.CardAction.Type));
            }
            else if (log.SkillAction != null)
            {
                para.Inlines.AddRange(RichTranslate(log.SkillAction));
            }
            para.Inlines.Add("判定结果为");
            para.Inlines.AddRange(RichTranslate(card, false));
            return para;
        }

        internal static Paragraph RichTranslateJudgeResultEffectiveness(Player p, ActionLog log, bool isSuccess)
        {
            Paragraph para = new Paragraph();
            para.Inlines.Add(Translate(p) + "的");
            if (log.CardAction != null)
            {
                para.Inlines.AddRange(RichTranslate(log.CardAction.Type));
            }
            else if (log.SkillAction != null)
            {
                para.Inlines.AddRange(RichTranslate(log.SkillAction));
            }
            para.Inlines.Add(isSuccess ? "生效" : "失效");
            return para;
        }

        internal static Paragraph RichTranslateRecoverHealth(Player player, int delta)
        {
            Paragraph para = new Paragraph();
            para.Inlines.Add(new Run(Translate(player)));
            string recoverStr = string.Format("恢复了{0}点体力，体力为{1}", delta, player.Health);
            para.Inlines.Add(new Run(recoverStr) { Foreground = GreenBrush });
            return para;
        }

        internal static Paragraph RichTranslateChat(string playerName, string userName, string message)
        {
            Paragraph para = new Paragraph();
            string speaker = userName ?? string.Empty;
            if (!string.IsNullOrEmpty(playerName))
            {
                speaker = (speaker == string.Empty) ? playerName : string.Format("{0}({1})", playerName, userName);
            }
            if (speaker != string.Empty)
            {
                para.Inlines.Add(new Run(speaker + ": ") { Foreground = DarkGreenBrush });
            }
            para.Inlines.AddRange(RichTranslateChatMessage(message));
            return para;
        }

        internal static IList<Inline> RichTranslateChatMessage(string message)
        {
            IList<Inline> inlines = new List<Inline>();
            var parts = message.Split('#');
            bool isFirst = false;
            foreach (var part in parts)
            {
                int index;
                if (isFirst || part.Length < 2 ||
                    !int.TryParse(part.Substring(0, 2), out index))
                {
                    inlines.Add(new Run(part));
                    continue;
                }
                string key = string.Format("Facial.{0}.Image", index);
                if (!_imageDict.Contains(key))
                {
                    inlines.Add(new Run(part));
                    continue;
                }
                else
                {
                    InlineUIContainer container = new InlineUIContainer(new Image() { Source = _imageDict[key] as ImageSource, Width = 18, Height = 18 });
                    inlines.Add(container);
                    if (part.Length > 2)
                    {
                        inlines.Add(new Run(part.Substring(2)));
                    }
                }
            }
            return inlines;
        }

        static ResourceDictionary _imageDict;

        static LogFormatter()
        {
            _imageDict = new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/Resources;component/Images/SystemImages.xaml")
            };
        }
    }
}

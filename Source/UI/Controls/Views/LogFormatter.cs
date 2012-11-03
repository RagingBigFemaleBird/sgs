using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanguosha.Core.Players;
using System.Windows;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using System.Diagnostics;

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

        
    }
}

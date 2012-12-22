using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Sanguosha.UI.Controls
{
    public class SkillButtonTemplateSelector : DataTemplateSelector
    {
        private static ResourceDictionary dict;
        static SkillButtonTemplateSelector()
        {
            dict = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Controls;component/Views/Skills/SkillButtonStyles.xaml") };
        }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            SkillCommand command = item as SkillCommand;
            if (command == null) return null;
            DataTemplate template = null;
            if (command is GuHuoSkillCommand)
            {
                template = dict["GuHuoButtonTemplate"] as DataTemplate;
            }
            else if (command.HeroName != null)
            {
                template = dict["RulerGivenSkillButtonTemplate"] as DataTemplate;
            }
            else
            {
                template = dict["SkillButtonTemplate"] as DataTemplate;
            }           
            return template;
        }
    }
}

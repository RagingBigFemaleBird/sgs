using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Sanguosha.UI.Controls
{
    public class CardChoiceBoxTemplateSelector : DataTemplateSelector
    {        
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            CardChoiceViewModel choiceModel = item as CardChoiceViewModel;
            if (choiceModel == null) return null;
            
            var results = from line in choiceModel.CardStacks
                          where line.IsResultDeck
                          select line;
            if (results.Count() > 0)
            {
                return MultiCardChoiceBoxTemplate;
            }
            else
            {
                return SingleCardChoiceBoxTemplate;
            }            
        }

        public DataTemplate MultiCardChoiceBoxTemplate { get; set; }

        public DataTemplate SingleCardChoiceBoxTemplate { get; set; }
    }
}

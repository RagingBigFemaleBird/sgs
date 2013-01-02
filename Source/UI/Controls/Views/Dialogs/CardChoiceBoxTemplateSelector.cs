using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Sanguosha.UI.Controls
{
    public class CardChoiceBoxSelector
    {
        public static FrameworkElement CreateBox(CardChoiceViewModel choiceModel)
        {
            if (choiceModel == null) return null;
            
            var results = from line in choiceModel.CardStacks
                          where line.IsResultDeck
                          select line;
            if (results.Count() > 0)
            {
                return new CardArrangeBox() { DataContext = choiceModel, IsHitTestVisible = !choiceModel.DisplayOnly };
            }
            else
            {
                return new CardChoiceBox() { DataContext = choiceModel, IsHitTestVisible = !choiceModel.DisplayOnly };
            }            
        }

    }
}

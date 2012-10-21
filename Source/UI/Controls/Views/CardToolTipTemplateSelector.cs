using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

using Sanguosha.Core.Cards;


namespace Sanguosha.UI.Controls
{

    public class CardToolTipTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BasicCardToolTipTemplate { get; set; }
        public DataTemplate WeaponToolTipTemplate { get; set; }
        public DataTemplate ArmorToolTipTemplate { get; set; }
        public DataTemplate DefensiveHorseToolTipTemplate { get; set; }
        public DataTemplate OffensiveHorseToolTipTemplate { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null) return base.SelectTemplate(item, container);

            CardViewModel viewModel = item as CardViewModel;
            if (viewModel == null)
            {
                Trace.TraceWarning("Trying to apply card tooltip template on an object that is not CardViewModel");
            }
            else if (viewModel.Card != null)
            {
                if (CardCategoryManager.IsCardCategory(viewModel.Category, CardCategory.Weapon))
                {
                    return WeaponToolTipTemplate;
                }
                else if (CardCategoryManager.IsCardCategory(viewModel.Category, CardCategory.Armor))
                {
                    return ArmorToolTipTemplate;
                }
                else if (CardCategoryManager.IsCardCategory(viewModel.Category, CardCategory.DefensiveHorse))
                {
                    return DefensiveHorseToolTipTemplate;
                }
                else if (CardCategoryManager.IsCardCategory(viewModel.Category, CardCategory.OffensiveHorse))
                {
                    return OffensiveHorseToolTipTemplate;
                }
                else
                {
                    return BasicCardToolTipTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}

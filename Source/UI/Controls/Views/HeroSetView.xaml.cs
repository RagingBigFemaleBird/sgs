using System;
using System.Collections.Generic;
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
using Sanguosha.Core.Cards;
using Sanguosha.Core.Heroes;

namespace Sanguosha.UI.Controls
{
	/// <summary>
	/// Interaction logic for HeroSetView.xaml
	/// </summary>
	public partial class HeroSetView : UserControl
	{
		public HeroSetView()
		{
			this.InitializeComponent();
		}

		private void gridHeroSet_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			// TODO: Add event handler implementation here.
            var model = gridHeroSet.SelectedItem as HeroViewModel;
			if (model != null)
			{
                heroCardView.DataContext = new CardViewModel()
                {
                    Card = new Card()
                    {
                        Id = model.Id                        
                    }
                };
                heroCardView.Visibility = Visibility.Visible;
				gridHeroInfo.Visibility = Visibility.Visible;
			}
			else
			{
                heroCardView.Visibility = Visibility.Collapsed;
				gridHeroInfo.Visibility = Visibility.Collapsed;
			}
		}
	}
}
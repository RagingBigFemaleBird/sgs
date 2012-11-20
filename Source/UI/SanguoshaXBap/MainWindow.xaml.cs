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
using Sanguosha.Core.Games;
using Sanguosha.Expansions.Basic;
using Sanguosha.Expansions.Battle;
using Sanguosha.Expansions.Wind;
using Sanguosha.Expansions.Fire;
using OverKnightFame12;
using Sanguosha.Expansions.OverKnightFame11;
using Hills;
using Sanguosha.Expansions.Woods;
using Sanguosha.Expansions.SP;
using System.IO;

namespace SanguoshaXBap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Page
    {
        private static string[] _resourceFiles = new string[] 
        {
            "Basic.Cards.xaml", "Basic.Skills.xaml", "Basic.Game.xaml",
            "Battle.Cards.xaml", "Battle.Game.xaml", "Fire.Game.xaml",
            "Fire.Skills.xaml", "Hills.Game.xaml", "Hills.Skills.xaml",
            "OverKnightFame11.Game.xaml", "OverKnightFame12.Game.xaml",
            "OverKnightFame11.Skills.xaml", "OverKnightFame12.Skills.xaml",
            "SP.Cards.xaml", "SP.Skills.xaml", "SP.Game.xaml",
            "Wind.Skills.xaml", "Wind.Game.xaml",
            "Woods.Skills.xaml", "Woods.Game.xaml", "Core.Game.xaml"
        };

        private static Expansion[] _expansions = new Expansion[]
        {
            new BasicExpansion(), new BattleExpansion(), new WindExpansion(), new FireExpansion(),
            new WoodsExpansion(), new HillsExpansion(), new OverKnightFame11Expansion(), new OverKnightFame12Expansion(),
            new SpExpansion()
        };

        private void _LoadResources()
        {
            foreach (var filePath in _resourceFiles)
            {
                try
                {
                    Uri uri = new Uri(string.Format("pack://siteoforigin:,,,/Resources/Texts/{0}", filePath));
                    Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = uri });
                }
                catch (BadImageFormatException)
                {
                    continue;
                }
                catch (FileNotFoundException)
                {
                    continue;
                }
            }
        }

        private void _LoadExpansions()
        {
            foreach (var expansion in _expansions)
            {
                GameEngine.LoadExpansion(expansion.GetType().Name, expansion);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _LoadResources();
            _LoadExpansions();
            MainFrame.Navigate(new Uri("pack://application:,,,/Sanguosha;component/MainGame.xaml"));
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace Tests
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            if (!PreloadCompleted) _Load();
            InitializeComponent();
        }

        private static string[] _dictionaryNames = new string[] { "Cards.xaml", "Skills.xaml", "Game.xaml" };

        private void _LoadResources(string folderPath)
        {
            try
            {
                var files = Directory.GetFiles(string.Format("{0}/Texts", folderPath));
                foreach (var filePath in files)
                {
                    if (!_dictionaryNames.Any(fileName => filePath.Contains(fileName))) continue;
                    try
                    {
                        Uri uri = new Uri(string.Format("pack://siteoforigin:,,,/{0}", filePath));
                        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = uri });
                    }
                    catch (BadImageFormatException)
                    {
                        continue;
                    }
                }
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/Resources;component/Lobby.xaml") });
            }
            catch (DirectoryNotFoundException)
            {
            }
            PreloadCompleted = true;
        }

        private void _Load()
        {
            _LoadResources(ResourcesFolder);

            GameEngine.LoadExpansions(ExpansionFolder);

        }

        public static string ExpansionFolder = "./";
        public static string ResourcesFolder = "Resources";

        private static bool _preloadCompleted = false;

        internal static bool PreloadCompleted
        {
            get { return _preloadCompleted; }
            set
            {
                _preloadCompleted = value;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            PlayerViewTest window = new PlayerViewTest();
            window.Show();
        }
    }
}

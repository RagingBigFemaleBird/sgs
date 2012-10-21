using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Sanguosha.Core.Games;
using System.IO;
using System.Reflection;

namespace Sanguosha
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static string[] _dictionaryNames = new string[] { "Cards.xaml", "Skills.xaml", "Game.xaml" };

        private void _LoadResources(string folderPath)
        {
            var files = Directory.GetFiles(folderPath);
            foreach (var file in files)
            {
                if (!file.EndsWith(".xaml")) continue;
                try
                {
                    Uri uri = new Uri(string.Format("pack://siteoforigin:,,,/{0}", folderPath, file));
                    Resources.MergedDictionaries.Add(new ResourceDictionary(){ Source = uri });
                }
                catch (BadImageFormatException)
                {
                    continue;
                }
            }
        }

        public static string ExpansionFolder = "Expansions";
        public static string ResourcesFolder = "Resources/Texts";

        protected override void OnStartup(StartupEventArgs e)
        {
            _LoadResources(ExpansionFolder);
            GameEngine.LoadExpansions(ExpansionFolder);
        }
    }
}

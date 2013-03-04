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
using System.IO;
using Sanguosha.Core.Games;
using System.Threading;
using System.Diagnostics;
using wyDay.Controls;

namespace Sanguosha.UI.Main
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        public MainWindow()
        {            
            InitializeComponent();
            automaticUpdater.ForceCheckForUpdate();
            automaticUpdater.ReadyToBeInstalled += (o, e) => { automaticUpdater.InstallNow(); };
            this.MainFrame.NavigationService.Navigated += NavigationService_Navigated;
        }

        void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            this.MainFrame.NavigationService.RemoveBackEntry();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            // MainFrame.Navigate(new Uri("pack://application:,,,/Sanguosha;component/MainGame.xaml"));
        }
    }
}

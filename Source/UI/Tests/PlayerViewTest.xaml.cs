using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Players;
using Sanguosha.UI.Controls;

namespace Tests
{
    /// <summary>
    /// Interaction logic for PlayerViewTest.xaml
    /// </summary>
    public partial class PlayerViewTest : Window
    {
        public PlayerViewTest()
        {
            InitializeComponent();
            InitPlayers();
        }

        private void InitPlayers()
        {
            playerView1.DataContext = new PlayerViewModel() { Player = new Player() { Hero = new Hero("LiuBei", true, Allegiance.Shu, 4) } };
            playerView2.DataContext = new PlayerViewModel() { Player = new Player() { Hero = new Hero("Caocao", true, Allegiance.Wei, 4) } };
            playerView3.DataContext = new PlayerViewModel() { Player = new Player() { Hero = new Hero("HuaXiong", true, Allegiance.Qun, 6), IsIronShackled = true } };
            playerView4.DataContext = new PlayerViewModel() { Player = new Player() { Hero = new Hero("SunQuan", true, Allegiance.Wu, 4), IsImprisoned = true } };
            playerView5.DataContext = new PlayerViewModel() { Player = new Player() { Hero = new Hero("ZhangJiao", true, Allegiance.Qun, 3) } };
            playerView6.DataContext = new PlayerViewModel() { Player = new Player() { Hero = new Hero("WangYi", true, Allegiance.Wei, 3), IsImprisoned = true, IsIronShackled = true } };
            playerView7.DataContext = new PlayerViewModel() { Player = new Player() { Hero = new Hero("SimaYi", true, Allegiance.Wei, 3) } };
            playerView8.DataContext = new PlayerViewModel() { Player = new Player() 
            {
                Hero = new Hero("LuSu", true, Allegiance.Wu, 3),
                Hero2 = new Hero("DiaoChan", true, Allegiance.Qun, 3),
            } };
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            foreach (var child in gridPlayers.Children)
            {
                var pv = (child as PlayerView);
                pv.PlayerModel.IsFaded = !pv.PlayerModel.IsFaded;
            }
        }
    }
}

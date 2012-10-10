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

using Sanguosha.Core.Players;

namespace Sanguosha.UI.Controls
{
    public enum GameTableLayout
    {
        Regular,
        Pk3v3,
        Pk1v3
    }

    /// <summary>
    /// Interaction logic for GameTable.xaml
    /// </summary>
    public partial class GameTable : UserControl
    {
        protected static int[][] regularSeatIndex;
        protected static int[][] pk3v3SeatIndex;
        protected static int[][] pk1v3SeatIndex;

        static GameTable()
        {
            // Layout:
            //    col1           col2
            // _______________________
            // |_Y_|______4_______|_X_| row1
            // |   |              |   |
            // | 2 |    table     | 0 | 
            // |___|______________|___|
            // |      dashboard       |
            // ------------------------
            // 1 = Y4X, 3=0X, 5=2Y

            regularSeatIndex = new int[][]
            {
                new int[]{4, 0, 0, 0, 0, 0, 0, 0, 0},    // 2 players
                new int[]{3, 5, 0, 0, 0, 0, 0, 0, 0}, // 3 players
                new int[]{3, 4, 5, 0, 0, 0, 0, 0, 0},
                new int[]{0, 4, 4, 2, 0, 0, 0, 0, 0},
                new int[]{0, 4, 4, 4, 2, 0, 0, 0, 0},
                new int[]{3, 3, 4, 4, 5, 5, 0, 0, 0},
                new int[]{3, 3, 4, 4, 4, 5, 5, 0, 0}, // 8 players
                new int[]{0, 0, 1, 1, 1, 1, 2, 2, 0}, // 9 players
                new int[]{0, 0, 1, 1, 1, 1, 1, 2, 2} // 10 players
            };
            pk3v3SeatIndex = new int[][]
            {
                new int[]{1, 1, 1}, // if self is lubu
                new int[]{0, 0, 4},
                new int[]{0, 4, 2},
                new int[]{4, 2, 2}
            };
            pk1v3SeatIndex = new int[][]
            {
                new int[]{0, 4, 4, 4, 2}, // lord        
                new int[]{4, 4, 4, 2, 2}, // rebel (left), same with loyalist (left)
                new int[]{0, 0, 4, 4, 4} // loyalist (right), same with rebel (right)
            };
        }

        public GameTable()
        {
            InitializeComponent();
            stackPanels = new List<StackPanel>() { stackPanel0, stackPanel1, stackPanel2, stackPanel3, stackPanel4, stackPanel5 };
            profileBoxes = new List<PlayerInfoView>();            
        }

        List<StackPanel> stackPanels;

        List<PlayerInfoView> profileBoxes;


        private GameTableLayout tableLayout;

        public GameTableLayout TableLayout
        {
            get { return tableLayout; }
            set 
            {
                tableLayout = value; 
                UpdateLayout(); 
            }
        }

        private int selfSeatNumber;

        public int SelfSeatNumber
        {
            get { return selfSeatNumber; }
            set 
            {
                selfSeatNumber = value;
                UpdateLayout();
            }
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            int[] seatMap = null;

            int numPlayers = profileBoxes.Count;
            
            if (numPlayers == 0) return arrangeBounds;

            if (numPlayers >= 10)
            {
                throw new NotImplementedException("Only fewer than 10 players are supported by UI.");
            }

            // Generate seat map.
            switch (TableLayout)
            {
                case GameTableLayout.Regular:
                    seatMap = regularSeatIndex[numPlayers];
                    break;
                case GameTableLayout.Pk3v3:
                    seatMap = pk3v3SeatIndex[(SelfSeatNumber - 1) % 3];
                    break;
                case GameTableLayout.Pk1v3:
                    seatMap = pk1v3SeatIndex[SelfSeatNumber - 1];
                    break;
            }

            // Add profile boxes to stack panels.
            foreach (StackPanel panel in stackPanels)
            {
                panel.Children.Clear();
            }

            for (int i = 0; i < numPlayers; i++)
            {
                int seat = seatMap[i];
                if (seat % 3 == 2)
                {
                    stackPanels[seat].Children.Add(profileBoxes[i]);
                }
                else
                {
                    stackPanels[seat].Children.Insert(0, profileBoxes[i]);
                }
            }

            // Adjust width/height of all controls
            int columns = Math.Max(
                Math.Max(stackPanel0.Children.Count + 1, stackPanel2.Children.Count + 1),
                Math.Max(stackPanel3.Children.Count, stackPanel5.Children.Count));

            int rows = Math.Max(
                stackPanel1.Children.Count, stackPanel4.Children.Count + 2);

            double width = (arrangeBounds.Width - horizontalSpacing * (columns - 1)) / columns;
            double height = (arrangeBounds.Height - verticalSpacing * (rows - 1)) / rows;

            double ratio = (double)Resources["PlayerInfoView.WidthHeightRatio"];

            width = Math.Min(width, height * ratio);
            height = height * ratio;

            foreach (var box in profileBoxes)
            {
                box.Width = width;
                box.Height = height;
            }

            rootGrid.RowDefinitions[0].Height = new GridLength(height);
            rootGrid.ColumnDefinitions[0].Width = new GridLength(width);
            rootGrid.ColumnDefinitions[2].Width = new GridLength(width);

            return arrangeBounds;
        }



        private int horizontalSpacing;
        public int HorizontalSpacing
        {
            get { return horizontalSpacing; }
            set
            {
                horizontalSpacing = value;
                UpdateLayout();
            }
        }

        private int verticalSpacing;
        public int VerticalSpacing 
        {
            get { return verticalSpacing; }
            set
            {
                verticalSpacing = value;
                UpdateLayout();
            }
        }

        public List<PlayerInfoView> ProfileBoxes
        {
            get
            {
                return profileBoxes;
            }
        }

        public List<Player> Players
        {
            set
            {                
                profileBoxes = new List<PlayerInfoView>();
                int num = value.Count;
                for (int i = 0; i < num; i++)
                {
                    PlayerInfoView box = new PlayerInfoView();
                    // box.Player = new UiPlayer(value[i]);
                    profileBoxes.Add(box);
                }
                UpdateLayout();
            }
        }
    }
}

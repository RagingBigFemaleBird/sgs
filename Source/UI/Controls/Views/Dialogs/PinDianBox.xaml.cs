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
using Sanguosha.Core.Players;
using System.Diagnostics;
using Sanguosha.Core.Cards;
using Sanguosha.UI.Animations;

namespace Sanguosha.UI.Controls
{
	/// <summary>
	/// Interaction logic for PinDianBox.xaml
	/// </summary>
	public partial class PinDianBox : UserControl
	{
		public PinDianBox()
		{
			this.InitializeComponent();
		}

        private Player _player1, _player2;

        public void StartPinDian(Player player1, Player player2)
        {
            cardLeft.DataContext = new CardSlotViewModel()
            {
                Hint = LogFormatter.Translate(player1),
                Card = null
            };

            cardRight.DataContext = new CardSlotViewModel()
            {
                Hint = LogFormatter.Translate(player2),
                Card = null
            };
            _player1 = player1;
            _player2 = player2;
        }

        public void OnPinDianCardPlayed(Player player)
        {
            Trace.Assert(_player1 != null && _player2 != null);
            CardView cardView = null;
            if (player == _player1) cardView = cardLeft;
            else 
            {
                Trace.Assert(player == _player2);
                cardView = cardRight;
            }
            Trace.Assert(cardView != null);
            cardView.DataContext = new CardViewModel() 
            {
                Card = new Card() { Id = Card.UnknownCardId },
                Footnote = LogFormatter.Translate(player),
                IsFootnoteVisible = true
            };
        }

        public void RevealResult(Card card1, Card card2)
        {
            cardLeft.DataContext = new CardViewModel() 
            {
                Card = card1,
                Footnote = LogFormatter.Translate(_player1),
                IsFootnoteVisible = true
            };
            cardRight.DataContext = new CardViewModel() 
            {
                Card = card2,
                Footnote = LogFormatter.Translate(_player2),
                IsFootnoteVisible = true
            };
            AnimationBase anim;
            if (card1.Rank > card2.Rank)
            {
                anim = new PinDianWinAnimation();
            }
            else
            {
                anim = new PinDianLoseAnimation();
            }
                        
            anim.SetValue(Canvas.LeftProperty, animationCenter.ActualWidth / 2 - anim.Width / 2);
            anim.SetValue(Canvas.TopProperty, animationCenter.ActualHeight / 2 - anim.Height / 2);
                        
            animationCenter.Children.Add(anim);
            anim.Completed += new EventHandler(anim_Completed);
            anim.Start();
        }

        public event EventHandler ResultShown;

        void anim_Completed(object sender, EventArgs e)
        {            
            var handler = ResultShown;
            if (handler != null)
            {
                handler(this, e);
            }
            cardLeft.DataContext = null;
            cardRight.DataContext = null;
        }
	}
}
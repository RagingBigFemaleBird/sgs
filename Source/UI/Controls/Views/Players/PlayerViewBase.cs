using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using Sanguosha.Core.Cards;
using System.Windows.Media;
using System.Windows.Input;
using Sanguosha.UI.Animations;
using Sanguosha.Core.Games;
using System.Threading;

namespace Sanguosha.UI.Controls
{
    public class PlayerViewBase : UserControl, IDeckContainer
    {
        public PlayerViewBase()
        {
            this.DataContextChanged += PlayerViewBase_DataContextChanged;
        }

        void PlayerViewBase_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateCards();
        }

        internal virtual void UpdateCards()
        {
            if (PlayerModel == null) return;
            PlayerModel.HandCards.Clear();
            PlayerModel.HandCardCount = 0;
            PlayerModel.WeaponCommand = null;
            PlayerModel.ArmorCommand = null;
            PlayerModel.DefensiveHorseCommand = null;
            PlayerModel.OffensiveHorseCommand = null;
            PlayerModel.PrivateDecks.Clear();

            var player = PlayerModel.Player;
            if (player == null) return;

            // HandCards
            foreach (var card in player.HandCards())
            {
                PlayerModel.HandCards.Add(new CardViewModel() { Card = card });
            }
            PlayerModel.HandCardCount = player.HandCards().Count;

            // Equipment
            foreach (var card in player.Equipments())
            {
                Equipment equip = card.Type as Equipment;

                if (equip != null)
                {
                    EquipCommand command = new EquipCommand() { Card = card };
                    switch (equip.Category)
                    {
                        case CardCategory.Weapon:
                            PlayerModel.WeaponCommand = command;
                            break;
                        case CardCategory.Armor:
                            PlayerModel.ArmorCommand = command;
                            break;
                        case CardCategory.DefensiveHorse:
                            PlayerModel.DefensiveHorseCommand = command;
                            break;
                        case CardCategory.OffensiveHorse:
                            PlayerModel.OffensiveHorseCommand = command;
                            break;
                    }
                }
            }

            // Private Decks
            var decks = Game.CurrentGame.Decks.GetPlayerPrivateDecks(player);
            foreach (var deck in decks)
            {
                var deckModel = new PrivateDeckViewModel();
                deckModel.Name = deck.Name;
                PlayerModel.PrivateDecks.Add(deckModel);
                var cards = Game.CurrentGame.Decks[player, deck];
                foreach (var card in cards)
                {
                    deckModel.Cards.Add(new CardViewModel() { Card = card });
                }
            }
        }

        #region Fields
        public PlayerViewModel PlayerModel
        {
            get
            {
                return DataContext as PlayerViewModel;
            }
        }

        private GameView parentGameView;

        public virtual GameView ParentGameView
        {
            get
            {
                return parentGameView;
            }
            set
            {
                parentGameView = value;
            }
        }
        #endregion

        #region Abstract Functions

        // The following functions are in its essence abstract function. They are not declared
        // abstract only to make designer happy to render their subclasses. (VS and blend will
        // not be able to create designer view for abstract class bases.

        protected virtual void AddHandCards(IList<CardView> cards, bool isFaked)
        {
        }

        protected virtual IList<CardView> RemoveHandCards(IList<Card> cards, bool isCopy)
        {
            return null;
        }

        protected virtual void AddPrivateCards(IList<CardView> cards, bool isFaked)
        {

        }

        protected virtual IEnumerable<CardView> RemovePrivateCards(IList<Card> cards)
        {
            return null;
        }

        protected virtual void AddEquipment(CardView card, bool isFaked)
        {
        }

        protected virtual CardView RemoveEquipment(Card card, bool isCopy)
        {
            return null;
        }

        protected virtual void AddDelayedTool(CardView card, bool isFaked)
        {
        }

        protected virtual CardView RemoveDelayedTool(Card card, bool isCopy)
        {
            return null;
        }

        protected virtual void AddRoleCard(CardView card, bool isFaked)
        {
        }

        protected virtual CardView RemoveRoleCard(Card card)
        {
            return null;
        }

        public void PlayAnimationAsync(AnimationBase animation, int playCenter, Point offset)
        {
            Application.Current.Dispatcher.BeginInvoke((ThreadStart)delegate()
            {
                PlayAnimation(animation, playCenter, offset);
            });
        }

        public virtual void PlayAnimation(AnimationBase animation, int playCenter, Point offset)
        {
        }

        public virtual void PlayIronShackleAnimation()
        {
        }

        public virtual void Tremble()
        {
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Compute card position on global canvas such that the card center is aligned to the center of <paramref name="element"/>.
        /// </summary>
        /// <param name="card">Card to be aligned.</param>
        /// <param name="element">FrameworkElement to be aligned to.</param>
        /// <returns>Position of card relative to global canvas.</returns>
        protected Point ComputeCardCenterPos(CardView card, FrameworkElement element)
        {
            double width = element.ActualWidth;
            double height = element.ActualHeight;
            if (width == 0) width = element.Width;
            if (height == 0) height = element.Height;
            Point dest = element.TranslatePoint(new Point(element.Width / 2, element.Height / 2),
                                                   ParentGameView.GlobalCanvas);
            dest.Offset(-card.Width / 2, -card.Height / 2);
            return dest;
        }
        #endregion

        #region CardMovement

        public void AddCards(DeckType deck, IList<CardView> cards, bool isFaked)
        {
            foreach (CardView card in cards)
            {
                card.CardModel.IsEnabled = false;
            }
            if (deck == DeckType.Hand)
            {
                foreach (var card in cards)
                {
                    PlayerModel.HandCards.Add(card.CardModel);
                }
                PlayerModel.HandCardCount += cards.Count;
                AddHandCards(cards, isFaked);
            }
            else if (deck == DeckType.Equipment)
            {
                foreach (var card in cards)
                {
                    Equipment equip = card.Card.Type as Equipment;

                    if (equip != null)
                    {
                        EquipCommand command = new EquipCommand() { Card = card.Card };
                        switch (equip.Category)
                        {
                            case CardCategory.Weapon:
                                PlayerModel.WeaponCommand = command;
                                break;
                            case CardCategory.Armor:
                                PlayerModel.ArmorCommand = command;
                                break;
                            case CardCategory.DefensiveHorse:
                                PlayerModel.DefensiveHorseCommand = command;
                                break;
                            case CardCategory.OffensiveHorse:
                                PlayerModel.OffensiveHorseCommand = command;
                                break;
                        }
                    }
                    AddEquipment(card, isFaked);
                }
            }
            else if (deck == DeckType.DelayedTools)
            {
                foreach (var card in cards)
                {
                    AddDelayedTool(card, isFaked);
                }
            }
            else if (deck == RoleGame.RoleDeckType)
            {
                foreach (var card in cards)
                {
                    AddRoleCard(card, isFaked);
                }
            }
            else if (deck is PrivateDeckType)
            {
                var deckModel = PlayerModel.PrivateDecks.FirstOrDefault(d => d.Name == deck.Name);
                if (deckModel == null)
                {
                    deckModel = new PrivateDeckViewModel();
                    deckModel.Name = deck.Name;
                    PlayerModel.PrivateDecks.Add(deckModel);
                }
                foreach (var card in cards)
                {
                    deckModel.Cards.Add(card.CardModel);
                }

                AddPrivateCards(cards, isFaked);
            }
            else
            {
                foreach (var card in cards)
                {
                    card.Disappear(isFaked ? 0d : 0.5d);
                }
            }
        }

        public IList<CardView> RemoveCards(DeckType deck, IList<Card> cards, bool isCopy)
        {
            List<CardView> cardsToRemove = new List<CardView>();
            if (deck == DeckType.Hand)
            {

                cardsToRemove.AddRange(RemoveHandCards(cards, isCopy));

                if (!isCopy)
                {
                    foreach (var card in cards)
                    {
                        var backup = new List<CardViewModel>(PlayerModel.HandCards);
                        foreach (var cardModel in backup)
                        {
                            if (cardModel.Card == card)
                            {
                                PlayerModel.HandCards.Remove(cardModel);
                            }
                        }
                    }
                    PlayerModel.HandCardCount -= cardsToRemove.Count;
                }
            }
            else if (deck == DeckType.Equipment)
            {
                foreach (var card in cards)
                {
                    Equipment equip = card.Type as Equipment;

                    if (equip != null)
                    {
                        switch (equip.Category)
                        {
                            case CardCategory.Weapon:
                                PlayerModel.WeaponCommand = null;
                                break;
                            case CardCategory.Armor:
                                PlayerModel.ArmorCommand = null;
                                break;
                            case CardCategory.DefensiveHorse:
                                PlayerModel.DefensiveHorseCommand = null;
                                break;
                            case CardCategory.OffensiveHorse:
                                PlayerModel.OffensiveHorseCommand = null;
                                break;
                        }
                    }

                    CardView cardView = RemoveEquipment(card, isCopy);
                    cardsToRemove.Add(cardView);

                }
            }
            else if (deck == DeckType.DelayedTools)
            {
                if (!ViewModelBase.IsDetached)
                {
                    foreach (var card in cards)
                    {
                        CardView cardView = RemoveDelayedTool(card, isCopy);
                        cardsToRemove.Add(cardView);
                    }
                }
            }
            else if (deck == RoleGame.RoleDeckType)
            {
                if (!ViewModelBase.IsDetached)
                {
                    foreach (var card in cards)
                    {
                        CardView cardView = RemoveRoleCard(card);
                        cardsToRemove.Add(cardView);
                    }
                }
            }
            else if (deck is PrivateDeckType)
            {
                var deckModel = PlayerModel.PrivateDecks.FirstOrDefault(d => d.Name == deck.Name);
                Trace.Assert(deckModel != null);

                if (!isCopy)
                {
                    foreach (var card in cards)
                    {
                        var cardModel = deckModel.Cards.First(c => c.Card == card);
                        Trace.Assert(cardModel != null, "Card cannot be found in the private deck");
                        deckModel.Cards.Remove(cardModel);
                    }
                    if (deckModel.Cards.Count == 0)
                    {
                        PlayerModel.PrivateDecks.Remove(deckModel);
                    }
                }

                cardsToRemove.AddRange(RemovePrivateCards(cards));
            }
            else
            {
                cardsToRemove.AddRange(RemoveHandCards(cards, isCopy));
            }

            foreach (var card in cardsToRemove)
            {
                card.CardModel.IsSelectionMode = false;
            }

            return cardsToRemove;
        }

        public virtual void UpdateCardAreas()
        {
        }
        #endregion

        #region UI Event Handlers
        protected void btnPrivateDeck_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var btn = sender as Button;
            var model = btn.DataContext as PrivateDeckViewModel;
            this.parentGameView.DisplayPrivateDeck(PlayerModel.Player, model);
        }
        #endregion

        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            return ClipToBounds ? base.GetLayoutClip(layoutSlotSize) : null;
        }


        public virtual void UpdateImpersonateStatus(bool isPrimaryHero)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Sanguosha.UI.Controls
{
    public class FluidCardStack : Panel
    {
        private object syncFocusedCard;
        public FluidCardStack()
        {
            syncFocusedCard = new object();
        }

        public double CardWidth
        {
            get { return (double)GetValue(CardWidthProperty); }
            set { SetValue(CardWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CardWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CardWidthProperty =
            DependencyProperty.Register("CardWidth", typeof(double), typeof(FluidCardStack), new UIPropertyMetadata(93d));

        public double CardHeight
        {
            get { return (double)GetValue(CardHeightProperty); }
            set { SetValue(CardHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CardWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CardHeightProperty =
            DependencyProperty.Register("CardHeight", typeof(double), typeof(FluidCardStack), new UIPropertyMetadata(130d));



        public double CardSpacing
        {
            get { return (double)GetValue(CardSpacingProperty); }
            set { SetValue(CardSpacingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CardSpacing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CardSpacingProperty =
            DependencyProperty.Register("CardSpacing", typeof(double), typeof(FluidCardStack), new UIPropertyMetadata(0d));

        

        public double HighlightItemExtraSpacing
        {
            get { return (double)GetValue(HighlightItemExtraSpacingProperty); }
            set { SetValue(HighlightItemExtraSpacingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HighlightItemExtraSpacing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightItemExtraSpacingProperty =
            DependencyProperty.Register("HighlightItemExtraSpacing", typeof(double), typeof(FluidCardStack), new UIPropertyMetadata(6d));

        
        protected override Size MeasureOverride(Size availableSize)
        {
            if (Children.Count == 0)
            {
                return new Size(0, 0);
            }
            Size resultSize = new Size(CardSpacing * (Children.Count - 1), 0);

            foreach (UIElement child in Children)
            {
                child.Measure(new Size(CardWidth, CardHeight));
                resultSize.Width += child.DesiredSize.Width;
                resultSize.Height = Math.Max(resultSize.Height, child.DesiredSize.Height);
            }

            resultSize.Width += 2 * HighlightItemExtraSpacing;

            if (resultSize.Width > availableSize.Width)
            {
                resultSize.Width = Math.Max(availableSize.Width, CardWidth);
            }
            
            return resultSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count == 0)
            {
                return new Size(0, 0);
            }
            int childCount = Children.Count;
            bool doExtraSpacing = (focusedCard != null);

            double width = Math.Min(finalSize.Width, CardWidth);
            double height = finalSize.Height;
            double space = 0;
            if (childCount > 1)
            {
                space = (finalSize.Width - width - 3 * HighlightItemExtraSpacing) / (childCount - 1);
            }
            double totalMargin = 0;
            bool spacingStart = false;
            for (int i = 0; i < childCount; i++)
            {
                UIElement card = Children[i];

                double spacing = 0;

                if (card == focusedCard)
                {
                    spacingStart = true;                    
                }
                else if (doExtraSpacing)
                {
                    if (!spacingStart)
                    {
                        spacing = -HighlightItemExtraSpacing;
                    }
                    else
                    {
                        spacing = HighlightItemExtraSpacing;
                    }
                }
                
                Thickness margin = (Thickness)card.GetValue(Panel.MarginProperty);

                card.Arrange(new Rect(HighlightItemExtraSpacing + i * space + totalMargin + spacing, 0, 
                             card.DesiredSize.Width + margin.Left + margin.Right, height));
                totalMargin += margin.Left + margin.Right;
            }
            return finalSize;
        }

        protected override void OnVisualChildrenChanged(
          DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            //For every element added 
            //add an event handler to know when it is clicked
            UIElement element = visualAdded as UIElement;
            if (element != null)
            {
                element.AddHandler(UIElement.MouseMoveEvent,
                        new RoutedEventHandler(element_MouseMove), true);            
                element.AddHandler(UIElement.MouseLeaveEvent,
                        new RoutedEventHandler(element_MouseLeave), true);
            }
        }

        private UIElement focusedCard;


        private void element_MouseMove(object sender, EventArgs args)
        {
            lock (syncFocusedCard)
            {
                UIElement element = sender as UIElement;
                if (element != null && element != focusedCard)
                {
                    var container = element as FrameworkElement;

                    if (container != null)
                    {
                        CardViewModel card = container.DataContext as CardViewModel;
                        if (card != null)
                        {
                            card.IsEnabled = true;
                            card.IsFaded = false;
                            foreach (var otherElement in Children)
                            {
                                container = otherElement as FrameworkElement;
                                if (container == null) continue;
                                CardViewModel otherCard = container.DataContext as CardViewModel;
                                if (otherCard != null && otherCard != card)
                                {
                                    otherCard.IsEnabled = false;
                                    otherCard.IsFaded = true;
                                }
                            }
                        }
                    }
                    focusedCard = element;
                    InvalidateArrange();
                }
            }
        }

        private void element_MouseLeave(object sender, EventArgs args)
        {
            lock (syncFocusedCard)
            {
                UIElement element = sender as UIElement;
                if (element != null && element == focusedCard)
                {
                    var container = element as FrameworkElement;
                    if (container != null)
                    {
                        CardViewModel card = container.DataContext as CardViewModel;
                        if (card != null)
                        {
                            card.IsEnabled = true;
                            card.IsFaded = false;
                            foreach (var anyElement in Children)
                            {
                                container = anyElement as FrameworkElement;
                                if (container == null) continue;
                                CardViewModel anyCard = container.DataContext as CardViewModel;
                                if (anyCard != null)
                                {
                                    anyCard.IsEnabled = true;
                                    anyCard.IsFaded = false;
                                }
                            }
                        }
                    }
                    focusedCard = null;
                    // InvalidateArrange();
                }
            }
        }
    }
}

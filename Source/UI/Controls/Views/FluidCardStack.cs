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



        public double HighlightItemExtraSpacing
        {
            get { return (double)GetValue(HighlightItemExtraSpacingProperty); }
            set { SetValue(HighlightItemExtraSpacingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HighlightItemExtraSpacing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightItemExtraSpacingProperty =
            DependencyProperty.Register("HighlightItemExtraSpacing", typeof(double), typeof(FluidCardStack), new UIPropertyMetadata(3d));

        
        protected override Size MeasureOverride(Size availableSize)
        {
            Size resultSize = new Size(0, 0);

            foreach (UIElement child in Children)
            {
                child.Measure(new Size(CardWidth, CardHeight));
                resultSize.Width += child.DesiredSize.Width;
                resultSize.Height = Math.Max(resultSize.Height, child.DesiredSize.Height);
            }

            if (resultSize.Width > availableSize.Width)
            {
                resultSize.Width = Math.Max(availableSize.Width, CardWidth);
            }
            
            return resultSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            int childCount = Children.Count;
            bool doExtraSpacing = (focusedCard != null);            

            double width = Math.Min(finalSize.Width, CardWidth);
            double height = finalSize.Height;
            double space = (finalSize.Width - width) / (childCount - 1);
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
                
                card.Arrange(new Rect(i * space + totalMargin + spacing, 0, 
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
                element.AddHandler(UIElement.MouseEnterEvent,
                        new RoutedEventHandler(element_MouseEnter), true);            
                element.AddHandler(UIElement.MouseLeaveEvent,
                        new RoutedEventHandler(element_MouseLeave), true);
            }
        }

        private UIElement focusedCard;

        private void element_MouseEnter(object sender, EventArgs args)
        {
            lock (syncFocusedCard)
            {
                UIElement element = sender as UIElement;
                if (element != null && element != focusedCard)
                {
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
                    focusedCard = null;
                    InvalidateArrange();
                }
            }
        }
    }
}

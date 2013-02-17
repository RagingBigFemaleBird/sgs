using System;
using System.Windows;
using System.Windows.Controls;

namespace Sanguosha.UI.Controls
{
    public class SkillButtonDock : WrapPanel
    {
        public SkillButtonDock()
        {
            ButtonsPerRow = 2;
        }

        private static int buttonHeight = 26;
        private static int dockWidth = 122;
        
        public int ButtonsPerRow
        {
            get;
            set;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            int numButtons = Children.Count;

            if (numButtons == 0)
            {
                return new Size(dockWidth, 0);
            }
            
            int rows = (numButtons - 1) / ButtonsPerRow + 1;
            double rowH = (int)Math.Min(buttonHeight, constraint.Height / rows);
            double rowW = constraint.Width;
            if (constraint.Width == double.PositiveInfinity)
            {
                rowW = dockWidth;
            }

            int[] btnNum = new int[rows + 1];
            int remainingBtns = numButtons;
            for (int i = 0; i < rows; i++)
            {
                btnNum[i] = Math.Min(ButtonsPerRow, remainingBtns);
                remainingBtns -= ButtonsPerRow;
            }

            // If the buttons in rows are 3, 1, then balance them to 2, 2
            if (rows >= 2)
            {
                if (btnNum[rows - 1] == 1 && btnNum[rows - 2] == 3)
                {
                    btnNum[rows - 1] = 2;
                    btnNum[rows - 2] = 2;
                }
            }
            else if (rows == 1 && btnNum[0] == 3)
            {
                btnNum[0] = 2;
                btnNum[1] = 1;
                rows = 2;
            }
            
            return new Size(rowW, rows * rowH);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            int numButtons = Children.Count;

            if (numButtons == 0)
            {
                return new Size(0, 0);
            }

            int rows = (numButtons - 1) / ButtonsPerRow + 1;
            int rowH = (int)Math.Min(buttonHeight, finalSize.Height / rows);
            double rowW = finalSize.Width;
            if (rowW == double.PositiveInfinity)
            {
                rowW = dockWidth;
            }

            int[] btnNum = new int[rows + 1];
            int remainingBtns = numButtons;
            for (int i = 0; i < rows; i++)
            {
                btnNum[i] = Math.Min(ButtonsPerRow, remainingBtns);
                remainingBtns -= ButtonsPerRow;
            }

            // If the buttons in rows are 3, 1, then balance them to 2, 2
            if (rows >= 2)
            {
                if (btnNum[rows - 1] == 1 && btnNum[rows - 2] == 3)
                {
                    btnNum[rows - 1] = 2;
                    btnNum[rows - 2] = 2;
                }
            }
            else if (rows == 1 && btnNum[0] == 3)
            {
                btnNum[0] = 2;
                btnNum[1] = 1;
                rows = 2;
            }

            int m = 0;
            for (int i = 0; i < rows; i++)
            {
                int rowTop = i * rowH;
                double btnWidth = (double)rowW / btnNum[i];
                for (int j = 0; j < btnNum[i]; j++)
                {
                    Children[m].Measure(new Size(btnWidth, rowH));
                    Children[m++].Arrange(new Rect(btnWidth * j, rowTop, btnWidth, rowH));
                }
            }

            return finalSize;
        }
    }
}

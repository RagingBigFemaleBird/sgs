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
using System.Windows.Media.Effects;
using System.Timers;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Sanguosha.UI.Controls
{
    /// <summary>
    /// Interaction logic for KeyEventNotifier.xaml
    /// </summary>
    public partial class KeyEventNotifierView : UserControl
    {
        DispatcherTimer _cleanUpCounter;
        int _currentTime;

        public KeyEventNotifierView()
        {
            this.InitializeComponent();
            _timeStamps = new List<int>();
            _cleanUpCounter = new DispatcherTimer(DispatcherPriority.ContextIdle);
            _cleanUpCounter.Interval = TimeSpan.FromSeconds(1.0);
            _cleanUpCounter.Tick += _cleanUpCounter_Elapsed;
            _cleanUpCounter.Start();
            _currentTime = 0;
            _disappearing = new List<bool>();
        }

        static int _cleanUpElapsedTimeThreshold = 3;

        private void _MakeDisappear(UIElement element)
        {
            DoubleAnimation anim = new DoubleAnimation();
            anim.To = 0d;
            anim.Duration = TimeSpan.FromMilliseconds(200d);
            element.BeginAnimation(UIElement.OpacityProperty, anim);
        }

        private void _MakeAppear(UIElement element)
        {
            DoubleAnimation anim = new DoubleAnimation();
            anim.To = 1d;
            anim.Duration = TimeSpan.FromMilliseconds(200d);
            element.BeginAnimation(UIElement.OpacityProperty, anim);
        }

        private void _cleanUpCounter_Elapsed(object sender, EventArgs e)
        {                        
            _currentTime++;
            Trace.Assert(_timeStamps.Count == _disappearing.Count);
            Trace.Assert(_timeStamps.Count == spLogs.Children.Count);
            int pivot = -1;
            for (int i = 0; i < _timeStamps.Count; i++)
            {
                if (_currentTime - _timeStamps[i] <= _cleanUpElapsedTimeThreshold) break;
                else
                {
                    if (_currentTime - _timeStamps[i] > _cleanUpElapsedTimeThreshold + 1) pivot = i;
                    if (!_disappearing[i]) _MakeDisappear(spLogs.Children[i]);
                    _disappearing[i] = true;
                }
            }
            if (pivot >= 0 && pivot < _timeStamps.Count)
            {
                _timeStamps.RemoveRange(0, pivot + 1);
                _disappearing.RemoveRange(0, pivot + 1);
                spLogs.Children.RemoveRange(0, pivot + 1);
            }            
        }

        private UIElement _BuildLogBlock(FlowDocument log)
        {
            log.FontFamily = new FontFamily("SimSun");
            log.FontSize = 15;            
            double width = log.GetFormattedText().WidthIncludingTrailingWhitespace + 20;
            RichTextBox rtb = new RichTextBox()
            {
                Document = log,
                Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xEE, 0x99)),
                Background = new SolidColorBrush(Colors.Transparent),
                BorderThickness = new Thickness(0d),                
                Width = width,
                Effect = new DropShadowEffect() { Color = Colors.Black, BlurRadius = 3, ShadowDepth = 0 },                
            };
            
            Border border = new Border()
            {
                CornerRadius = new CornerRadius(3d),
                Child = rtb,
                Width = width,
                Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x3D, 0x3A, 0x2C)),
                BorderThickness = new Thickness(1d),
                Opacity = 0d,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };
           
            return border;
        }

        List<int> _timeStamps;
        List<bool> _disappearing;

        public void AddLog(FlowDocument log)
        {
            var logEntry = _BuildLogBlock(log);
            
            _timeStamps.Add(_currentTime);
            _disappearing.Add(false);
            spLogs.Children.Add(logEntry);

            _MakeAppear(logEntry);
        }
    }
}
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

namespace Sanguosha.UI.Controls
{
    /// <summary>
    /// Interaction logic for KeyEventNotifier.xaml
    /// </summary>
    public partial class KeyEventNotifierView : UserControl
    {
        Timer _cleanUpCounter;
        int _currentTime;

        public KeyEventNotifierView()
        {
            this.InitializeComponent();
            _timeStamps = new List<int>();
            _cleanUpCounter = new Timer(1000);
            _cleanUpCounter.AutoReset = true;
            _cleanUpCounter.Elapsed += _cleanUpCounter_Elapsed;
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

        private void _cleanUpCounter_Elapsed(object sender, ElapsedEventArgs e)
        {            
            Application.Current.Dispatcher.Invoke((System.Threading.ThreadStart)delegate()
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
            });
        }

        private UIElement _BuildLogBlock(FlowDocument log)
        {            
            RichTextBox rtb = new RichTextBox()
            {
                Document = log,
                Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xEE, 0x99)),
                Background = new SolidColorBrush(Colors.Transparent),
                BorderThickness = new Thickness(0d),
                FontFamily = new FontFamily("SimSun"),
                FontSize = 14,
                Effect = new DropShadowEffect() { Color = Colors.Black, BlurRadius = 3, ShadowDepth = 0 },
                Opacity = 0d
            };
            
            return rtb;
        }

        List<int> _timeStamps;
        List<bool> _disappearing;

        public void AddLog(FlowDocument log)
        {
            /*
                   <Border CornerRadius="2" Background="#FF3D3A2C" HorizontalAlignment="Center">
                            <RichTextBox Document="{Binding}" Foreground="#FFFFEE99" FontFamily="SimSun" FontSize="14">
                                <RichTextBox.Effect>
                                    <DropShadowEffect Color="Black" BlurRadius="3" ShadowDepth="0" />
                                </RichTextBox.Effect>
                            </RichTextBox>
                    </Border>
             */
            var logEntry = _BuildLogBlock(log);
            
            _timeStamps.Add(_currentTime);
            _disappearing.Add(false);
            spLogs.Children.Add(logEntry);

            _MakeAppear(logEntry);
        }
    }
}
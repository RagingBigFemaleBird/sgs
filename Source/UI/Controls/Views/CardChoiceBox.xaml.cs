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
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Diagnostics;

namespace Sanguosha.UI.Controls
{
	/// <summary>
	/// Interaction logic for CardChoiceBox.xaml
	/// </summary>
	public partial class CardChoiceBox : UserControl
	{
		public CardChoiceBox()
		{
			this.InitializeComponent();
            this.DataContextChanged += CardChoiceBox_DataContextChanged;
            _modelPropertyChangedHandler = new PropertyChangedEventHandler(model_PropertyChanged);
        }

        private PropertyChangedEventHandler _modelPropertyChangedHandler;

        void CardChoiceBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldModel = e.OldValue as CardChoiceViewModel;
            if (oldModel != null)
            {
                oldModel.PropertyChanged -= _modelPropertyChangedHandler;
            }

            var newModel = e.NewValue as CardChoiceViewModel;
            if (newModel != null)
            {
                newModel.PropertyChanged += _modelPropertyChangedHandler;
            }
        }

        private void model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var model = sender as CardChoiceViewModel;
            Trace.Assert(model != null);
            if (e.PropertyName == "TimeOutSeconds")
            {
                Duration duration = new Duration(TimeSpan.FromSeconds(model.TimeOutSeconds));
                DoubleAnimation doubleanimation = new DoubleAnimation(100d, 0d, duration);
                progressBar.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);
            }
        }
    }
}
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
using System.Windows.Media.Animation;

namespace Sanguosha.UI.Animations
{
    /// <summary>
    /// Interaction logic for LuanWuAnimation.xaml
    /// </summary>
    public partial class ExcitingSkillAnimation : AnimationBase
    {
        private class ExcitingSkillAnimationViewModel : Sanguosha.UI.Controls.ViewModelBase
        {
            private string _skillName;
            public string SkillName 
            {
                get
                {
                    return _skillName;
                }
                set
                {
                    if (_skillName == value) return;
                    _skillName = value;
                    OnPropertyChanged("SkillName");
                }
            }
            private string _heroName;
            public string HeroName
            {
                get
                {
                    return _heroName;
                }
                set
                {
                    if (_heroName == value) return;
                    _heroName = value;
                    OnPropertyChanged("HeroName");
                }
            }
        }

        private ExcitingSkillAnimationViewModel _model;
        public ExcitingSkillAnimation()
        {
            this.InitializeComponent();
            mainAnimation = Resources["mainAnimation"] as Storyboard;
            _model = new ExcitingSkillAnimationViewModel();	        
            DataContext = _model;
        }

        public string HeroName
        {
            get
            {
                return _model.HeroName;
            }
            set
            {
                _model.HeroName = value;               
            }
        }

        public string SkillName
        {
            get
            {
                return _model.SkillName;
            }
            set
            {
                _model.SkillName = value;    
            }
        }
    }
}
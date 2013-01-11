using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Sanguosha.Core.Utils;

namespace Sanguosha.UI.Controls
{
    public class ReplayControllerViewModel : ViewModelBase
    {
        public ReplayControllerViewModel()
        {
            SlowDownCommand = new SimpleRelayCommand((o) =>
            {
                double speed = Controller.Speed;
                if (speed <= 0.5d) return;
                if (speed == 1.0d) speed = 0.5d;
                else speed -= 1.0d;
                Controller.Speed = speed;
                OnPropertyChanged("SpeedString");
            }) { CanExecuteStatus = true };
            SpeedUpCommand = new SimpleRelayCommand((o) =>
            {
                double speed = Controller.Speed;
                if (speed >= 8.0) return;
                if (speed == 0.5d) speed = 1.0d;
                else speed += 1.0d;
                Controller.Speed = speed;
                OnPropertyChanged("SpeedString");
            }) { CanExecuteStatus = true };
        }

        public ReplayControllerViewModel(ReplayController controller) : this()
        {
            Controller = controller;            
        }

        public ReplayController Controller
        {
            get;
            set;
        }
                
        public bool IsPaused 
        {
            get
            {
                return Controller.IsPaused;
            }
            set
            {
                if (Controller.IsPaused == value) return;
                else
                {

                    if (value)
                    {
                        Controller.Pause();
                    }
                    else
                    {
                        Controller.Resume();
                    }
                }
                OnPropertyChanged("IsPaused");
            }
        }

        public string SpeedString
        {
            get
            {
                return Controller.Speed.ToString("F1");    
            }
        }

        public ICommand SpeedUpCommand { get; set; }

        public ICommand SlowDownCommand { get; set; }

        public bool EvenDelays 
        {
            get
            {
                return Controller.EvenDelays;
            }
            set
            {
                if (Controller.EvenDelays == value) return;
                Controller.EvenDelays = value;
                OnPropertyChanged("EvenDelays");
            }
        }
    }
}

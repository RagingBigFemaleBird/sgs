using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sanguosha.Core.Utils
{
    public class ReplayController
    {
        public static readonly int EvenReplayBaseSpeedInMs = 2000;
        Semaphore pauseResume;

        public ReplayController()
        {
            Speed = 1.0d;
            pauseResume = new Semaphore(1, 1);
        }

        public void Lock()
        {
            pauseResume.WaitOne();
        }

        public void Unlock()
        {
            pauseResume.Release(1);
        }

        public void Pause()
        {
            if (IsPaused) return;
            IsPaused = true;
            pauseResume.WaitOne();
        }
        
        public void Resume()
        {
            if (!IsPaused) return;
            pauseResume.Release(1);
            IsPaused = false;
        }

        public bool IsPaused { get; private set; }
        public double Speed { get; set; }
        public bool EvenDelays { get; set; }
        public bool NoDelays { get; set; }
    }
}

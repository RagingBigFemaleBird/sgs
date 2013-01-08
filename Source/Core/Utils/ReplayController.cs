using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sanguosha.Core.Utils
{
    public class ReplayController
    {
        public static readonly int EvenReplayBaseSpeedInMs = 1000;
        Semaphore pauseResume;
        public ReplayController()
        {
            pauseResume = new Semaphore(1, 2);
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
            pauseResume.WaitOne();
        }
        public void Resume()
        {
            pauseResume.Release(1);
        }
        public double Speed { get; set; }
        public bool EvenDelays { get; set; }
        public bool NoDelays { get; set; }
    }
}

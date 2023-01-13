using System;

namespace PRN
{

    public class Timer
    {

        public TimeSpan remainingTime { get; private set; }

        public event Action onLoop;

        public Timer(TimeSpan remainingTime)
        {
            this.remainingTime = remainingTime;
        }

        public void Tick(TimeSpan deltaTime)
        {
            if (remainingTime == TimeSpan.Zero)
                return;
            remainingTime -= deltaTime;
            if (remainingTime < TimeSpan.Zero)
            {
                remainingTime = TimeSpan.Zero;
                onLoop?.Invoke();
            }
        }
    }

}
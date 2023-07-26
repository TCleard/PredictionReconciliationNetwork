using System;

namespace PRN
{

    public class Ticker
    {

        public TimeSpan loopDuration { get; private set; }
        public TimeSpan remainingTime { get; private set; }

        public event Action onTick;

        public Ticker(TimeSpan loopDuration)
        {
            this.loopDuration = loopDuration;
            remainingTime = loopDuration;
        }

        public void OnTimePassed(TimeSpan deltaTime)
        {
            remainingTime -= deltaTime;
            while (remainingTime <= TimeSpan.Zero)
            {
                remainingTime += loopDuration;
                onTick?.Invoke();
            }
        }
    }

}
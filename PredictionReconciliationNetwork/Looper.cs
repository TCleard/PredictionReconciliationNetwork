using System;

namespace PRN
{

    public class Looper
    {

        public TimeSpan loopDuration { get; private set; }
        public TimeSpan remainingTime { get; private set; }

        public event Action onLoop;

        public Looper(TimeSpan loopDuration)
        {
            this.loopDuration = loopDuration;
            remainingTime = loopDuration;
        }

        public void Tick(TimeSpan deltaTime)
        {
            remainingTime -= deltaTime;
            while (remainingTime <= TimeSpan.Zero)
            {
                remainingTime += loopDuration;
                onLoop?.Invoke();
            }
        }
    }

}
using System;

namespace PRN
{

    namespace Actor
    {

        public abstract class Actor
        {

            protected int tick { get; private set; } = -1;
            protected TimeSpan tickDeltaTime { get; private set; }

            public Actor(Looper looper)
            {
                tickDeltaTime = looper.loopDuration;
                looper.onLoop += () => {
                    tick++;
                    OnTick();
                };
            }

            protected abstract void OnTick();

        }

    }

}
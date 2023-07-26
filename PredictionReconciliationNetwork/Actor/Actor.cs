using System;

namespace PRN.Actor
{

    public abstract class Actor
    {

        private Ticker ticker;

        protected int tick { get; private set; } = -1;
        protected TimeSpan tickDeltaTime { get; private set; }
    
        public Actor(Ticker ticker)
        {
            this.ticker = ticker;
            tickDeltaTime = ticker.loopDuration;
            ticker.onTick += InternalOnTick;
        }

        private void InternalOnTick() {
            tick++;
            OnTick();
        }
    
        protected abstract void OnTick();

        public void Dispose() {
            ticker.onTick -= InternalOnTick;
        }
    
    }

}
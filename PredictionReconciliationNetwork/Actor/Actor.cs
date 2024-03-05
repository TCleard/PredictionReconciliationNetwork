using System;

namespace PRN.Actor {

    public abstract class Actor {

        private Ticker ticker;

        protected int tick { get; private set; } = -1;
        protected TimeSpan tickDeltaTime => ticker.loopDuration;

        public Actor(Ticker ticker) {
            this.ticker = ticker;
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
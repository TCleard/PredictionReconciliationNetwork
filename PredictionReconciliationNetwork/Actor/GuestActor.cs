using System;
using System.Collections.Generic;

namespace PRN.Actor
{

    public class GuestActor<I, S>: Actor
        where I : Input
        where S : State
    {

        private Processor<I, S> processor;

        private Queue<S> stateQueue;
        private S lastState;

        public event Action<S> onStateUpdate;

        public GuestActor(
            Ticker ticker,
            Processor<I, S> processor
        ) : base(ticker)
        {
            this.processor = processor;
            stateQueue = new Queue<S>();
        }

        public void OnServerStateReceived(S state)
        {
            stateQueue.Enqueue(state);
        }

        protected override void OnTick()
        {
            while (stateQueue.Count > 0)
            {
                lastState = stateQueue.Dequeue();
                processor.Rewind(lastState);
                onStateUpdate?.Invoke(lastState);
            }
        }

    }

}
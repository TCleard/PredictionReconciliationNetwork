using System;
using System.Collections.Generic;

namespace PRN.Actor
{

    public class GuestActor<I, S>: Actor
        where I : IInput
        where S : IState
    {

        private IProcessor<I, S> processor;

        private Queue<S> stateQueue;

        public event Action<S> onStateUpdate;

        public GuestActor(
            Ticker ticker,
            IProcessor<I, S> processor
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
            if (stateQueue.Count > 0)
            {
                if (stateQueue.Count > 20)
                {
                    S state = stateQueue.Dequeue();
                    for (int i = 0; i < stateQueue.Count; i++)
                    {
                        state = stateQueue.Dequeue();
                    }
                    processor.Rewind(state);
                    onStateUpdate?.Invoke(state);
                }
                else if (stateQueue.Count > 5)
                {
                    stateQueue.Dequeue();
                    S state = stateQueue.Dequeue();
                    processor.Rewind(state);
                    onStateUpdate?.Invoke(state);
                }
                else
                {
                    S lastState = stateQueue.Dequeue();
                    processor.Rewind(lastState);
                    onStateUpdate?.Invoke(lastState);
                }
            }
        }

    }

}
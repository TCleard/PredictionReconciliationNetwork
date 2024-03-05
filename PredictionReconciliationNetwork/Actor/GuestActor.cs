using System;
using System.Collections.Generic;

namespace PRN.Actor {

    public class GuestActor<I, S>: Actor
        where I : IInput
        where S : IState {

        private IProcessor<I, S> processor;

        private StateSyncPolicy stateSyncPolicy;

        private I lastInput;
        private Queue<S> stateQueue;

        public event Action<S> onState;

        public GuestActor(
            Ticker ticker,
            IProcessor<I, S> processor,
            StateSyncPolicy stateSyncPolicy
        ) : base(ticker) {
            this.processor = processor;
            stateQueue = new Queue<S>();
            this.stateSyncPolicy = stateSyncPolicy;
        }

        public void OnServerInputStateReceived(I input, S state) {
            lastInput = input;
            stateQueue.Enqueue(state);
        }

        protected override void OnTick() {

            if (stateQueue.Count >= stateSyncPolicy.catchUpThreshold) {
                while (stateQueue.Count > 0) {
                    S state = stateQueue.Dequeue();
                    processor.Rewind(state);
                    onState?.Invoke(state);
                }
            } else if (stateQueue.Count > 0) {
                S state = stateQueue.Dequeue();
                processor.Rewind(state);
                onState?.Invoke(state);
            }

            if (stateSyncPolicy.withPrediction)
                onState?.Invoke(processor.Process(lastInput, tickDeltaTime));

        }

    }

}
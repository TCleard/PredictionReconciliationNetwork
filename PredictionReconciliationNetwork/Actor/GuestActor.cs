using System;
using System.Collections.Generic;

namespace PRN.Actor {

    public class GuestActor<I, S>: Actor
        where I : IInput
        where S : IState {

        private IProcessor<I, S> processor;

        private I lastInput;
        private Queue<S> stateQueue;

        public GuestActor(
            Ticker ticker,
            IProcessor<I, S> processor
        ) : base(ticker) {
            this.processor = processor;
            stateQueue = new Queue<S>();
        }

        public void OnServerInputStateReceived(I input, S state) {
            lastInput = input;
            stateQueue.Enqueue(state);
        }

        protected override void OnTick() {
            while (stateQueue.Count > 0)
                processor.Rewind(stateQueue.Dequeue());
            processor.Process(lastInput, tickDeltaTime);
        }

    }

}
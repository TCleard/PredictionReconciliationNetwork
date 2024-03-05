using System;
using System.Collections.Generic;

namespace PRN.Actor {

    public class ServerActor<I, S>: Actor
        where I : IInput
        where S : IState {

        protected IProcessor<I, S> processor;

        private int maxInputCount = 0;
        private Queue<I> inputQueue;

        private I lastInput;

        private S[] stateBuffer;

        public event Action<I, S> onInputStateUpdate;

        public event Action onTickerHackerDetected;

        public ServerActor(
            Ticker ticker,
            IProcessor<I, S> processor,
            int bufferSize
            ) : base(ticker) {
            this.processor = processor;
            inputQueue = new Queue<I>();
            stateBuffer = new S[bufferSize];
        }

        public void OnInputReceived(I input) {
            // If owner sends more inputs that he's supposed to
            // (as the server is behind, inputQueue.Count should NEVER be greater than maxInputCount)
            // It means he changed his handler ticker value
            // So we ignore the extra inputs
            if (inputQueue.Count < maxInputCount)
                inputQueue.Enqueue(input);
            else
                onTickerHackerDetected?.Invoke();
        }

        protected override void OnTick() {

            while (inputQueue.Count > 0) {
                lastInput = inputQueue.Dequeue();
                S state = processor.Process(lastInput, tickDeltaTime);
                state.SetTick(lastInput.GetTick());
                stateBuffer[lastInput.GetTick() % stateBuffer.Length] = state;
                onInputStateUpdate?.Invoke(lastInput, state);
                maxInputCount--;
            }

            maxInputCount++;
        }

    }

}
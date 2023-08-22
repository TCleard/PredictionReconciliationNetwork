using System;
using System.Collections.Generic;

namespace PRN.Actor
{

    public class ServerActor<I, S>: Actor
        where I: IInput
        where S: IState
    {

        protected IProcessor<I, S> processor;

        private Queue<I> inputQueue;

        private I lastInput;
        private S[] stateBuffer;

        public event Action<S> onStateUpdate;

        public ServerActor(
            Ticker ticker,
            IProcessor<I, S> processor,
            int bufferSize
            ) : base(ticker)
        {
            this.processor = processor;
            inputQueue = new Queue<I>();
            stateBuffer = new S[bufferSize];
        }

        public void OnInputReceived(I input)
        {
            inputQueue.Enqueue(input);
        }

        protected override void OnTick()
        {
            while (inputQueue.Count > 0)
            {
                lastInput = inputQueue.Dequeue();
                S state = processor.Process(lastInput, tickDeltaTime);
                state.SetTick(lastInput.GetTick());
                stateBuffer[lastInput.GetTick() % stateBuffer.Length] = state;
                onStateUpdate?.Invoke(state);
            }
        }

    }

}
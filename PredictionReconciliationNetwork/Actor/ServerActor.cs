using System;
using System.Collections.Generic;

namespace PRN.Actor
{

    public class ServerActor<I, S>: Actor
        where I: IInput
        where S: IState
    {

        protected IProcessor<I, S> processor;


        public Queue<I> inputQueue;

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
            if (inputQueue.Count > 0)
            {
                if (inputQueue.Count > 20)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        lastInput = inputQueue.Dequeue();
                        S state = processor.Process(lastInput, tickDeltaTime);
                        state.SetTick(lastInput.GetTick());
                        stateBuffer[lastInput.GetTick() % stateBuffer.Length] = state;
                        onStateUpdate?.Invoke(state);
                    }
                }
                else if (inputQueue.Count > 5) 
                {
                    lastInput = inputQueue.Dequeue();
                    S firstState = processor.Process(lastInput, tickDeltaTime);
                    firstState.SetTick(lastInput.GetTick());
                    stateBuffer[lastInput.GetTick() % stateBuffer.Length] = firstState;
                    onStateUpdate?.Invoke(firstState);

                    lastInput = inputQueue.Dequeue();
                    S lastState = processor.Process(lastInput, tickDeltaTime);
                    lastState.SetTick(lastInput.GetTick());
                    stateBuffer[lastInput.GetTick() % stateBuffer.Length] = lastState;
                    onStateUpdate?.Invoke(lastState);
                }
                else
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

}
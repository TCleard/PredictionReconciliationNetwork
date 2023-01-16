using System;
using System.Collections.Generic;

namespace PRN
{

    namespace Actor
    {

        public class ServerActor<I, S>: Actor
            where I: Input
            where S: State
        {

            protected Processor<I, S> processor;

            private Queue<I> inputQueue;

            private I lastInput;
            private S[] stateBuffer;

            public event Action<S> onStateUpdate;

            public ServerActor(
                Looper looper,
                Processor<I, S> processor,
                int bufferSize
                ) : base(looper)
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

}
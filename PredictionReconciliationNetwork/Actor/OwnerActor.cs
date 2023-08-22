using System;

namespace PRN.Actor
{

    public class OwnerActor<I, S> : Actor
        where I : Input
        where S : State
    {

        protected Processor<I, S> processor;
        protected InputProvider<I> inputProvider;
        protected StateConsistencyChecker<S> consistencyChecker;

        private int bufferSize;

        private I[] inputBuffer;
        private S[] stateBuffer;

        private S lastServerState;
        private S lastProcessedState;

        public event Action<I> onInputUpdate;
        public event Action<S> onStateUpdate;

        public OwnerActor(
            Ticker ticker,
            Processor<I, S> processor,
            InputProvider<I> inputProvider,
            StateConsistencyChecker<S> consistencyChecker,
            int bufferSize
            ) : base(ticker)
        {
            this.processor = processor;
            this.inputProvider = inputProvider;
            this.consistencyChecker = consistencyChecker;
            this.bufferSize = bufferSize;
            inputBuffer = new I[bufferSize];
            stateBuffer = new S[bufferSize];
        }

        public void OnServerStateReceived(S state)
        {
            lastServerState = state;
        }

        protected override void OnTick()
        {
            if (
                !Equals(default(S), lastServerState)
                && (Equals(default(S), lastProcessedState) || !Equals(lastServerState, lastProcessedState))
            )
            {
                Reconcile();
            }

            int bufferIndex = tick % bufferSize;

            I input = inputProvider.GetInput(tickDeltaTime);
            input.SetTick(tick);
            inputBuffer[bufferIndex] = input;

            S state = processor.Process(input, tickDeltaTime);
            state.SetTick(tick);
            stateBuffer[bufferIndex] = state;

            onInputUpdate?.Invoke(input);
            onStateUpdate?.Invoke(state);

        }

        private void Reconcile()
        {
            lastProcessedState = lastServerState;

            int serverStateBufferIndex = lastServerState.GetTick() % bufferSize;

            if (!consistencyChecker.IsConsistent(lastServerState, stateBuffer[serverStateBufferIndex]))
            {
                // Rewind to latest coherent state
                processor.Rewind(lastServerState);

                // Update buffer at index of latest server state
                stateBuffer[serverStateBufferIndex] = lastServerState;

                // Re processing states until last input
                S rewindedState;
                int rewindedStateBufferIndex;
                int tickToProcess = lastServerState.GetTick() + 1;
                while (tickToProcess < tick)
                {
                    rewindedState = processor.Process(inputBuffer[tickToProcess % bufferSize], tickDeltaTime);
                    rewindedStateBufferIndex = tickToProcess % bufferSize;
                    stateBuffer[rewindedStateBufferIndex] = rewindedState;
                    tickToProcess++;
                }
            }
        }

    }

}
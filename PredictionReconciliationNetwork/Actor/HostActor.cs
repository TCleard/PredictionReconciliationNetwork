using System;

namespace PRN
{

    namespace Actor
    {

        public class HostActor<I, S>: Actor
            where I: Input
            where S: State
        {

            private I lastInput;
            private S lastState;

            private Processor<I, S> processor;
            private InputProvider<I> inputProvider;

            public event Action<S> onStateUpdate;

            public HostActor(
                Looper looper,
                Processor<I, S> processor,
                InputProvider<I> inputProvider
                ) : base(looper)
            {
                this.processor = processor;
                this.inputProvider = inputProvider;
            }

            protected override void OnTick()
            {
                lastInput = inputProvider.GetInput();
                lastState = processor.Process(lastInput, tickDeltaTime);
                onStateUpdate?.Invoke(lastState);
            }

        }

    }

}
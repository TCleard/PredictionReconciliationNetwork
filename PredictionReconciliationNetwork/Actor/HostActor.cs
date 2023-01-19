using System;
using PRN;

namespace PRN.Actor
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
            lastInput.SetTick(tick);
            lastState = processor.Process(lastInput, tickDeltaTime);
            lastState.SetTick(tick);
            onStateUpdate?.Invoke(lastState);
        }

    }

}
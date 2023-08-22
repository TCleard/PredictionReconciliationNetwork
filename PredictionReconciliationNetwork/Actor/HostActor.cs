using System;
using PRN;

namespace PRN.Actor
{

    public class HostActor<I, S>: Actor
        where I: IInput
        where S: IState
    {

        private I lastInput;
        private S lastState;

        private IProcessor<I, S> processor;
        private IInputProvider<I> inputProvider;

        public event Action<S> onStateUpdate;

        public HostActor(
            Ticker ticker,
            IProcessor<I, S> processor,
            IInputProvider<I> inputProvider
            ) : base(ticker)
        {
            this.processor = processor;
            this.inputProvider = inputProvider;
        }

        protected override void OnTick()
        {
            lastInput = inputProvider.GetInput(tickDeltaTime);
            lastInput.SetTick(tick);
            lastState = processor.Process(lastInput, tickDeltaTime);
            lastState.SetTick(tick);
            onStateUpdate?.Invoke(lastState);
        }

    }

}
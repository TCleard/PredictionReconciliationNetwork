using System;

namespace PRN.Actor {

    public class HostActor<I, S>: Actor
        where I : IInput
        where S : IState {

        private I lastInput;
        private S lastState;

        private IProcessor<I, S> processor;
        private IInputProvider<I> inputProvider;

        public event Action<I, S> onInputStateUpdate;

        public HostActor(
            Ticker ticker,
            IProcessor<I, S> processor,
            IInputProvider<I> inputProvider
            ) : base(ticker) {
            this.processor = processor;
            this.inputProvider = inputProvider;
        }

        protected override void OnTick() {
            lastInput = inputProvider.GetInput();
            lastInput.SetTick(tick);
            lastState = processor.Process(lastInput, tickDeltaTime);
            lastState.SetTick(tick);
            onInputStateUpdate?.Invoke(lastInput, lastState);
        }

    }

}
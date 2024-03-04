using PRN.Actor;
using System;

namespace PRN {

    public class NetworkHandler<I, S>
        where I : IInput
        where S : IState {

        private ServerActor<I, S> serverActor;
        private HostActor<I, S> hostActor;
        private OwnerActor<I, S> ownerActor;
        private GuestActor<I, S> guestActor;

        public event Action<I> onSendInputToServer;
        public event Action<I, S> onSendInputStateToClient;

        public event Action<S> onState;

        public NetworkHandler(
            NetworkRole role,
            Ticker ticker,
            IProcessor<I, S> processor,
            IInputProvider<I> inputProvider,
            IStateConsistencyChecker<S> consistencyChecker,
            StateSyncPolicy stateSyncPolicy = null,
            int bufferSize = 512
        ) {
            switch (role) {
                case NetworkRole.SERVER:
                    serverActor = new ServerActor<I, S>(
                        ticker: ticker,
                        processor: processor,
                        bufferSize: bufferSize
                    );
                    serverActor.onInputStateUpdate += (input, state) => {
                        onSendInputStateToClient?.Invoke(input, state);
                        onState?.Invoke(state);
                    };
                    break;
                case NetworkRole.HOST:
                    hostActor = new HostActor<I, S>(
                        ticker: ticker,
                        processor: processor,
                        inputProvider: inputProvider
                    );
                    hostActor.onInputStateUpdate += (input, state) => {
                        onSendInputStateToClient?.Invoke(input, state);
                        onState?.Invoke(state);
                    };
                    break;
                case NetworkRole.OWNER:
                    ownerActor = new OwnerActor<I, S>(
                        ticker: ticker,
                        processor: processor,
                        inputProvider: inputProvider,
                        consistencyChecker: consistencyChecker,
                        bufferSize: bufferSize
                    );
                    ownerActor.onInputUpdate += (input) => {
                        onSendInputToServer?.Invoke(input);
                    };
                    ownerActor.onStateUpdate += (state) => {
                        onState?.Invoke(state);
                    };
                    break;
                case NetworkRole.GUEST:
                    guestActor = new GuestActor<I, S>(
                        ticker: ticker,
                        processor: processor,
                        stateSyncPolicy: stateSyncPolicy != null ? stateSyncPolicy : new StateSyncPolicy()
                    );
                    guestActor.onState += (state) => {
                        onState?.Invoke(state);
                    };
                    break;
            }
        }

        public void OnOwnerInputReceived(I input) {
            if (serverActor != null)
                serverActor.OnInputReceived(input);
        }

        public void OnServerInputStateReceived(I input, S state) {
            if (ownerActor != null)
                ownerActor.OnServerStateReceived(state);
            if (guestActor != null)
                guestActor.OnServerInputStateReceived(input, state);
        }

        public void Dispose() {
            if (serverActor != null)
                serverActor.Dispose();
            if (hostActor != null)
                hostActor.Dispose();
            if (ownerActor != null)
                ownerActor.Dispose();
            if (guestActor != null)
                guestActor.Dispose();
        }

    }

}
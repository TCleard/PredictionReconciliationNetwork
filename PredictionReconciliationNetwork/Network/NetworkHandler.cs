using PRN.Actor;
using System;

namespace PRN
{

    public class NetworkHandler<I, S>
        where I : IInput
        where S : IState
    {

        private ServerActor<I, S> serverActor;
        private HostActor<I, S> hostActor;
        private OwnerActor<I, S> ownerActor;
        private GuestActor<I, S> guestActor;

        public event Action<I> onSendInputToServer;
        public event Action<S> onSendStateToClient;

        public event Action<S> onState;

        public NetworkHandler(
            NetworkRole role,
            Ticker ticker,
            IProcessor<I, S> processor,
            IInputProvider<I> inputProvider,
            IStateConsistencyChecker<S> consistencyChecker,
            int bufferSize = 512
        )
        {
            switch (role)
            {
                case NetworkRole.SERVER:
                    serverActor = new ServerActor<I, S>(
                        ticker: ticker,
                        processor: processor,
                        bufferSize: bufferSize
                    );
                    serverActor.onStateUpdate += (state) =>
                    {
                        onSendStateToClient?.Invoke(state);
                        onState?.Invoke(state);
                    };
                    break;
                case NetworkRole.HOST:
                    hostActor = new HostActor<I, S>(
                        ticker: ticker,
                        processor: processor,
                        inputProvider: inputProvider
                    );
                    hostActor.onStateUpdate += (state) =>
                    {
                        onSendStateToClient?.Invoke(state);
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
                    ownerActor.onInputUpdate += (input) =>
                    {
                        onSendInputToServer?.Invoke(input);
                    };
                    ownerActor.onStateUpdate += (state) =>
                    {
                        onState?.Invoke(state);
                    };
                    break;
                case NetworkRole.GUEST:
                    guestActor = new GuestActor<I, S>(
                        ticker: ticker,
                        processor: processor
                    );
                    guestActor.onStateUpdate += (state) =>
                    {
                        onState?.Invoke(state);
                    };
                    break;
            }
        }

        public void OnOwnerInputReceived(I input)
        {
            if (serverActor != null)
                serverActor.OnInputReceived(input);
        }

        public void OnServerStateReceived(S state)
        {
            if (ownerActor != null)
                ownerActor.OnServerStateReceived(state);
            if (guestActor != null)
                guestActor.OnServerStateReceived(state);
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
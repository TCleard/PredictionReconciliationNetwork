using PRN.Actor;
using System;

namespace PRN
{

    namespace Network
    {

        public class NetworkHandler<I, S>
            where I : Input
            where S : State
        {

            private ServerActor<I, S> serverActor;
            private HostActor<I, S> hostActor;
            private OwnerActor<I, S> ownerActor;
            private ClientActor<I, S> clentActor;

            public event Action<I> onSendInputToServer;
            public event Action<S> onSendStateToClient;

            public event Action<S> onState;

            public NetworkHandler(
                NetworkRole role,
                Looper looper,
                Processor<I, S> processor,
                InputProvider<I> inputProvider,
                StateConsistancyChecker<S> consistancyChecker,
                int bufferSize = 512
            )
            {
                switch (role)
                {
                    case NetworkRole.SERVER:
                        serverActor = new ServerActor<I, S>(
                            looper: looper,
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
                            looper: looper,
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
                            looper: looper,
                            processor: processor,
                            inputProvider: inputProvider,
                            consistancyChecker: consistancyChecker,
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
                    case NetworkRole.CLIENT:
                        clentActor = new ClientActor<I, S>(
                            looper: looper,
                            processor: processor
                        );
                        clentActor.onStateUpdate += (state) =>
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
                if (clentActor != null)
                    clentActor.OnServerStateReceived(state);
            }

        }

    }

}

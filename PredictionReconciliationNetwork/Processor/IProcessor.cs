using System;

namespace PRN {

    public interface IProcessor<I, S>
        where I : IInput
        where S : IState {

        S Process(I input, TimeSpan deltaTime);

        void Rewind(S state);

    }

}

using System;

namespace PRN
{

    public interface Processor<I, S>
        where I : Input
        where S : State
    {
    
        S Process(I input, TimeSpan deltaTime);
    
        void Rewind(S state);
    
    }

}

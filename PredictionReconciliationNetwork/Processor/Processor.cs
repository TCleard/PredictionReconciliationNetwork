using System;

namespace PRN
{

    public abstract class Processor<I, S>
        where I : Input
        where S : State
    {
    
        public abstract S Process(I input, TimeSpan deltaTime);
    
        public abstract void Rewind(S state);
    
    }

}

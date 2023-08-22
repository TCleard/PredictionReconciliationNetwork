using System;

namespace PRN
{

    public interface InputProvider<I>
        where I : Input
    {
    
        I GetInput(TimeSpan deltaTime);
    
    }

}
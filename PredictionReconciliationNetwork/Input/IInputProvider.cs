namespace PRN
{

    public interface IInputProvider<I>
        where I : IInput
    {
    
        I GetInput();
    
    }

}
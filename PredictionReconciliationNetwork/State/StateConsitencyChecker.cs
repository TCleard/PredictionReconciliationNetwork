namespace PRN {

    public interface IStateConsistencyChecker<S>
        where S : IState
    {
    
        bool IsConsistent(S serverState, S ownerState);
    
    }

}
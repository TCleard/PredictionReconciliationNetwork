namespace PRN {

    public interface StateConsistencyChecker<S>
        where S : State
    {
    
        bool IsConsistent(S serverState, S ownerState);
    
    }

}
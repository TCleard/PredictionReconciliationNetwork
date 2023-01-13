namespace PRN {

    public interface StateConsistancyChecker<S>
        where S : State
    {
    
        bool IsConsistant(S serverState, S ownerState);
    
    }

}
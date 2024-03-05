namespace PRN {

    public class StateSyncPolicy {

        public int catchUpThreshold { get; private set; }

        public bool withPrediction { get; private set; }

        public StateSyncPolicy(
            int catchUpThreshold = 10,
            bool withPrediction = false
        ) {
            this.withPrediction = withPrediction;
            this.catchUpThreshold = catchUpThreshold;
        }

    }

}

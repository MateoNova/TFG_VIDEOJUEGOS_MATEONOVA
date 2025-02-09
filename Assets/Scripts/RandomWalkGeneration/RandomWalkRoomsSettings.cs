namespace RandomWalkGeneration
{
    /// <summary>
    /// Settings for the random walk rooms generation.
    /// </summary>
    [System.Serializable]
    public class RandomWalkRoomsSettings
    {
        /// <summary>
        /// The number of iterations to perform the random walk.
        /// </summary>
        public int walkIterations = 10;

        /// <summary>
        /// The number of steps to take in each iteration of the random walk.
        /// </summary>
        public int stepsPerIteration = 10;

        /// <summary>
        /// Whether to randomize the starting position after each iteration.
        /// </summary>
        public bool randomizeStartPos = true;
    }
}
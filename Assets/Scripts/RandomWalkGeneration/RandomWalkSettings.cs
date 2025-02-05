[System.Serializable]
public class RandomWalkSettings
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
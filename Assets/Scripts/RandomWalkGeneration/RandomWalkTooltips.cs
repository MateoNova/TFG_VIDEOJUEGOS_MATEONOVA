namespace RandomWalkGeneration
{
    public static class RandomWalkTooltips
    {
        public const string WalkIterationsTooltip =
            "The number of iterations the random walk algorithm will perform." +
            " - More iterations will result in a larger and more complex dungeon";

        public const string StepsPerIterationTooltip =
            "The number of steps taken in each iteration of the random walk algorithm." +
            " - More steps per iteration will create longer paths in the dungeon.";


        public const string GenerateCorridorsTooltip =
            "Flag to determine if corridors should be generated and calculate potential room positions." +
            " - If true, the generator will create corridors and rooms" +
            " - Otherwise, it will create a single walkable area.";


        public const string CorridorLengthTooltip =
            "Length of each corridor. " +
            " - Smaller values create shorter corridors, resulting in a more compact dungeon layout." +
            " - Larger values create longer corridors, resulting in a more spread-out dungeon layout.";

        public const string CorridorCountTooltip =
            "Number of corridors to generate. " +
            " - Smaller values create fewer corridors, resulting in fewer connections between rooms. " +
            " - Larger values create more corridors, resulting in more connections between rooms.";

        public const string RoomPercentageTooltip =
            "Percentage of potential room positions to convert into rooms. " +
            " - Smaller values create fewer rooms, resulting in a more sparse dungeon layout. " +
            " - Larger values create more rooms, resulting in a more dense dungeon layout.";

        public const string CorridorWidthTooltip =
            "Width of the corridors. " +
            " - Smaller values create narrower corridors, resulting in tighter passageways." +
            " - Larger values create wider corridors, resulting in more spacious passageways.";
    }
}
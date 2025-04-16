namespace Controllers.Editor
{
    /// <summary>
    /// Static controller responsible for managing the state of spawn point selection.
    /// </summary>
    public static class SpawningController
    {
        /// <summary>
        /// Indicates whether the user is currently in the process of setting a spawn point.
        /// When true, the system allows interaction for selecting or moving the spawn point.
        /// </summary>
        public static bool IsSettingSpawnPoint = false;
    }
}
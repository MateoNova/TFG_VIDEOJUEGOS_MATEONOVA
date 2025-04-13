using Controllers.Generators;

namespace Models.Editor
{
    /// <summary>
    /// Singleton service responsible for managing the current dungeon generator.
    /// </summary>
    public class GeneratorService
    {
        /// <summary>
        /// The single instance of the GeneratorService.
        /// </summary>
        private static GeneratorService _instance;

        /// <summary>
        /// Gets the singleton instance of the GeneratorService.
        /// </summary>
        public static GeneratorService Instance => _instance ??= new GeneratorService();

        /// <summary>
        /// The currently active dungeon generator.
        /// </summary>
        public BaseGenerator CurrentGenerator { get; private set; }

        /// <summary>
        /// Private constructor to enforce the singleton pattern.
        /// </summary>
        private GeneratorService()
        {
        }

        /// <summary>
        /// Sets the current dungeon generator. If the generator changes, it triggers the GeneratorChanged event.
        /// </summary>
        /// <param name="generator">The new generator to set as the current generator.</param>
        public void SetCurrentGenerator(BaseGenerator generator)
        {
            if (CurrentGenerator == generator) return;

            CurrentGenerator = generator;
            EventBus.OnGeneratorChanged();
        }
    }
}
namespace Editor.Models
{
    public class GeneratorService
    {
        private static GeneratorService _instance;
        public static GeneratorService Instance => _instance ??= new GeneratorService();

        public BaseGenerator CurrentGenerator { get; private set; }

        private GeneratorService()
        {
        }

        public void SetCurrentGenerator(BaseGenerator generator)
        {
            if (CurrentGenerator == generator) return;
            CurrentGenerator = generator;
            EventBus.OnGeneratorChanged();
        }
    }
}
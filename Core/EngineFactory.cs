namespace FuzzyGameEngine.Core
{
    public static class EngineFactory
    {
        public static FuzzyEngine Create(AdaptiveController adaptive) =>
            new(
            [
                new Rule(x => Math.Min(x["skill_high"], x["accuracy_high"]), "high", "Професіонал"),
                new Rule(x => Math.Min(x["stress_high"], x["health_low"]), "low", "Перевантаження"),
                new Rule(x => Math.Min(x["reaction_fast"], x["skill_high"]), "high", "Швидка реакція"),
                new Rule(x => Math.Min(x["reaction_slow"], x["stress_high"]), "low", "Повільно + стрес"),
                new Rule(x => x["progress_late"], "high", "Пізній етап"),
                new Rule(x => 0.5, "medium", "Базове правило")
            ], adaptive);
    }
}
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
                new Rule(x => 0.35, "medium", "Базовий рівень складності"),
                new Rule(x => x["enemies_many"], "high", "Багато ворогів"),
                new Rule(x => x["damage_high"], "high", "Високі пошкодження"),
                new Rule(x => x["time_long"], "low", "Багато часу на рівень"), 
                new Rule(x => Math.Min(x["enemies_many"], x["damage_high"]), "high", "Вороги + пошкодження"),
                new Rule(x => Math.Min(x["time_long"], x["health_low"]), "low", "Час + низьке здоров'я"),
                new Rule(x => Math.Min(x["skill_high"], x["accuracy_high"]), "medium", "Хороша навичка + точність"),
                new Rule(x => x["progress_late"], "medium", "Пізній етап гри"),
                new Rule(x => Math.Max(x["enemies_many"], x["damage_high"]), "high", "Загроза від ворогів або пошкоджень"),
                new Rule(x => Math.Min(x["time_long"], x["health_low"]), "low", "Багато часу + низьке здоров'я"),
            ], adaptive);
    }
}
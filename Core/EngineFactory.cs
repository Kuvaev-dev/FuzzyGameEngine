namespace FuzzyGameEngine.Core
{
    public static class EngineFactory
    {
        public static FuzzyEngine Create(AdaptiveController adaptive)
        {
            return new FuzzyEngine(new List<Rule>
            {
                new Rule(x => Math.Min(x["time_high"], x["enemies_high"]), "high", "Hard level"),
                new Rule(x => Math.Min(x["health_low"], x["damage_high"]), "high", "Critical state"),
                new Rule(x => Math.Min(x["accuracy_high"], x["time_low"]), "low", "Pro player"),
                new Rule(x => x["health_high"], "low", "High HP"),
                new Rule(x => 0.5, "medium", "Default")
            }, adaptive);
        }
    }
}

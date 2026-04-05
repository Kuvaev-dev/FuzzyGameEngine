using System.Text;

namespace FuzzyGameEngine.Core
{
    public class FuzzyEngine
    {
        private readonly List<Rule> rules;
        private readonly AdaptiveController adaptive;

        public FuzzyEngine(List<Rule> rules, AdaptiveController adaptive)
        {
            this.rules = rules;
            this.adaptive = adaptive;
        }

        public (double Result, string Explain, string Log) Evaluate(GameState s)
        {
            var log = new StringBuilder();
            log.AppendLine("=== РОЗРАХУНОК НЕЧІТКОЇ СИСТЕМИ ===");
            log.AppendLine($"Час: {DateTime.Now:HH:mm:ss}");

            var f = Fuzzify(s);

            foreach (var item in f)
                log.AppendLine($" {item.Key,-18}: {item.Value:F3}");

            var output = new Dictionary<string, double>();

            var explain = new StringBuilder();

            foreach (var r in rules)
            {
                double strength = r.Condition(f);
                if (strength > 0.001)
                {
                    log.AppendLine($" Правило «{r.Description}» → сила {strength:F3}");
                    explain.AppendLine($"{r.Description}: {strength:F3}");
                }

                output[r.Output] = Math.Max(output.GetValueOrDefault(r.Output, 0), strength);
            }

            double resultBefore = Defuzzify(output);
            log.AppendLine($"До адаптації: {resultBefore:F3}");

            double result = adaptive.Apply(resultBefore);
            log.AppendLine($"Після адаптації: {result:F3}");

            log.AppendLine($"Адаптивний bias: {adaptive.GetBias():F3}");

            return (result, explain.ToString(), log.ToString());
        }

        private static Dictionary<string, double> Fuzzify(GameState s)
        {
            return new()
            {
                { "skill_high", Membership.Trap(s.PlayerSkill, 60, 75, 100, 100) },
                { "stress_high", Membership.Trap(s.StressLevel, 60, 80, 100, 100) },
                { "health_low", Membership.Trap(s.Health, 0, 0, 30, 50) },
                { "accuracy_high", Membership.Trap(s.Accuracy, 70, 85, 100, 100) },
                { "reaction_fast", Membership.Trap(s.ReactionTime, 0, 0, 200, 400) },
                { "reaction_slow", Membership.Trap(s.ReactionTime, 400, 600, 1000, 1000) },
                { "progress_late", Membership.Trap(s.Progress, 60, 80, 100, 100) }
            };
        }

        private static double Defuzzify(Dictionary<string, double> o)
        {
            double num = 0, den = 0;
            for (double x = 0; x <= 100; x += 1)
            {
                double low = Membership.Trap(x, 0, 0, 30, 50);
                double mid = Membership.Trap(x, 30, 50, 60, 80);
                double high = Membership.Trap(x, 70, 85, 100, 100);
                double mu = Math.Max(
                    Math.Min(low, o.GetValueOrDefault("low", 0)),
                    Math.Max(
                        Math.Min(mid, o.GetValueOrDefault("medium", 0)),
                        Math.Min(high, o.GetValueOrDefault("high", 0))
                    ));
                num += x * mu;
                den += mu;
            }
            return den == 0 ? 50 : num / den;
        }
    }
}
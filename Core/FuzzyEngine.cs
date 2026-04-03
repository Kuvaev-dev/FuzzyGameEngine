namespace FuzzyGameEngine.Core
{
    public class FuzzyEngine
    {
        private List<Rule> rules;
        private AdaptiveController adaptive;

        public FuzzyEngine(List<Rule> rules, AdaptiveController adaptive)
        {
            this.rules = rules;
            this.adaptive = adaptive;
        }

        public (double, string) Evaluate(GameState s)
        {
            var f = Fuzzify(s);
            var output = new Dictionary<string, double>();
            var explain = "";

            foreach (var r in rules)
            {
                double strength = r.Condition(f);

                if (strength > 0)
                    explain += $"{r.Description}: {strength:F2}\n";

                if (!output.ContainsKey(r.Output))
                    output[r.Output] = strength;
                else
                    output[r.Output] = Math.Max(output[r.Output], strength);
            }

            double result = Defuzzify(output);
            result = adaptive.Apply(result);

            return (result, explain);
        }

        private Dictionary<string, double> Fuzzify(GameState s)
        {
            return new Dictionary<string, double>
        {
            { "time_high", Membership.Trap(s.Time,80,120,300,300) },
            { "time_low", Membership.Trap(s.Time,0,0,40,80) },

            { "enemies_high", Membership.Trap(s.Enemies,5,10,20,20) },
            { "enemies_low", Membership.Trap(s.Enemies,0,0,3,6) },

            { "health_low", Membership.Trap(s.Health,0,0,30,50) },
            { "health_high", Membership.Trap(s.Health,50,70,100,100) },

            { "accuracy_low", Membership.Trap(s.Accuracy,0,0,40,60) },
            { "accuracy_high", Membership.Trap(s.Accuracy,60,80,100,100) },

            { "damage_high", Membership.Trap(s.DamageTaken,50,70,100,100) },
            { "damage_low", Membership.Trap(s.DamageTaken,0,0,20,40) }
        };
        }

        private double Defuzzify(Dictionary<string, double> o)
        {
            double num = 0, den = 0;

            for (double x = 0; x <= 100; x += 2)
            {
                double low = Membership.Trap(x, 0, 0, 30, 50);
                double mid = Membership.Trap(x, 30, 50, 60, 80);
                double high = Membership.Trap(x, 70, 85, 100, 100);

                double mu = Math.Max(
                    Math.Min(low, o.GetValueOrDefault("low")),
                    Math.Max(
                        Math.Min(mid, o.GetValueOrDefault("medium")),
                        Math.Min(high, o.GetValueOrDefault("high"))
                    )
                );

                num += x * mu;
                den += mu;
            }

            return den == 0 ? 0 : num / den;
        }
    }
}

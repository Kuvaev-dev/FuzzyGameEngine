namespace FuzzyGameEngine.Core
{
    public class Rule
    {
        public Func<Dictionary<string, double>, double> Condition { get; }
        public string Output { get; }
        public string Description { get; }

        public Rule(Func<Dictionary<string, double>, double> cond, string output, string desc)
        {
            Condition = cond;
            Output = output;
            Description = desc;
        }
    }
}
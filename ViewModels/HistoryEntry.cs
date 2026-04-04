namespace FuzzyGameEngine.ViewModels
{
    public class HistoryEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public double DifficultyValue { get; set; }
        public string Conclusion { get; set; } = string.Empty;
        public double Bias { get; set; }
        public double Time { get; set; }
        public double Enemies { get; set; }
        public double Health { get; set; }
        public double Accuracy { get; set; }
        public double Damage { get; set; }
        public double PlayerSkill { get; set; }
        public double StressLevel { get; set; }
        public double Progress { get; set; }
        public double ReactionTime { get; set; }
        public string Explain { get; set; } = string.Empty;

        public string DisplayText =>
            $"{Timestamp:T} | Складність: {DifficultyValue:F1} | {Conclusion} | Bias: {Bias:F2}";
    }
}
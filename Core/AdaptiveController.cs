namespace FuzzyGameEngine.Core
{
    public class AdaptiveController
    {
        private const int HistorySize = 15;
        private const double MinBias = 0.7;
        private const double MaxBias = 1.3;
        private const double BiasStep = 0.02;
        private double bias = 1.0;

        private readonly Queue<double> history = new();

        public double Apply(double value) => value * bias;

        public void Learn(double performance)
        {
            history.Enqueue(performance);
            if (history.Count > HistorySize) history.Dequeue();

            double avg = history.Any() ? history.Average() : 50;

            if (avg > 70)
                bias -= BiasStep;
            else if (avg < 40)
                bias += BiasStep;

            bias = Math.Clamp(bias, MinBias, MaxBias);
        }

        public double GetBias() => bias;
    }
}
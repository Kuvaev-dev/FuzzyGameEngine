namespace FuzzyGameEngine.Core
{
    public class AdaptiveController
    {
        private const int HistorySize = 15;
        private const double MinBias = 0.75;
        private const double MaxBias = 1.35;
        private const double BiasStep = 0.015;

        private double bias = 1.0;
        private readonly Queue<double> history = new();

        public double Apply(double value) => value * bias;

        public void Learn(double difficulty)
        {
            history.Enqueue(difficulty);
            if (history.Count > HistorySize)
                history.Dequeue();

            if (history.Count < 3)
                return; // не адаптуємося, поки мало даних

            double avg = history.Average();

            if (avg > 72)
                bias -= BiasStep;           // рівень часто складний → зменшуємо bias
            else if (avg < 48)
                bias += BiasStep;           // рівень часто легкий → збільшуємо bias
            else if (avg > 65)
                bias -= BiasStep * 0.4;     // слабка корекція
            else if (avg < 55)
                bias += BiasStep * 0.4;

            bias = bias * 0.92 + 1.0 * 0.08;

            bias = Math.Clamp(bias, MinBias, MaxBias);
        }

        public double GetBias() => bias;
    }
}
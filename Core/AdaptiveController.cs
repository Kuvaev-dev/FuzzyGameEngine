namespace FuzzyGameEngine.Core
{
    public class AdaptiveController
    {
        private double bias = 1.0;

        public double Apply(double value) => value * bias;

        public void Learn(double performance)
        {
            if (performance > 70) bias += 0.05;
            else bias -= 0.05;

            bias = Math.Clamp(bias, 0.5, 1.5);
        }
    }
}

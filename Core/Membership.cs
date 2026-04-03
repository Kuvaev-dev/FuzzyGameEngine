namespace FuzzyGameEngine.Core
{
    public static class Membership
    {
        public static double Trap(double x, double a, double b, double c, double d)
        {
            if (x <= a || x >= d) return 0;
            if (x >= b && x <= c) return 1;
            if (x < b) return (x - a) / (b - a);
            return (d - x) / (d - c);
        }
    }
}

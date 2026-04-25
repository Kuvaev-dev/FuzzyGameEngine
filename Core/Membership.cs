namespace FuzzyGameEngine.Core
{
    public static class Membership
    {
        // 1. Трапецієподібна функція
        public static double Trap(double x, double a, double b, double c, double d)
        {
            if (x <= a || x >= d) return 0;
            if (x >= b && x <= c) return 1;
            if (x < b) return (x - a) / (b - a);
            return (d - x) / (d - c);
        }

        // 2. Трикутна функція
        public static double Tri(double x, double a, double b, double c)
        {
            if (x <= a || x >= c) return 0;
            if (x == b) return 1;
            if (x < b) return (x - a) / (b - a);
            return (c - x) / (c - b);
        }

        // 3. Функція Гаусса
        public static double Gauss(double x, double center, double width)
        {
            if (width == 0) return x == center ? 1 : 0;
            return Math.Exp(-Math.Pow(x - center, 2) / (2 * Math.Pow(width, 2)));
        }
    }
}
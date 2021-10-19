using System.Linq;

namespace RegBot
{
    class Methods
    {
        public static decimal Median(decimal[] d)
        {
            var ys = d.OrderBy(x => x).ToList();
            double mid = (ys.Count - 1) / 2.0;
            return (ys[(int)(mid)] + ys[(int)(mid + 0.5)]) / 2;
        }
    }
}


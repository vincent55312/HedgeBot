using System;
using System.Linq;
using System.Collections.Generic;

namespace RegBot
{
    public class Pos
    {
        public decimal priceY;
        public decimal timeX;
        public Pos(decimal timeX, decimal priceY)
        {
            this.timeX = timeX;
            this.priceY = priceY;
        }
    }
    public class PosDv
    {
        public decimal priceY { get; set; }
        public decimal timeX { get; set; }
        public decimal gapAvg { get; set; }

        public PosDv(decimal timeX, decimal priceY, decimal deviationAvg)
        {
            this.timeX = timeX;
            this.priceY = priceY;
            this.gapAvg = deviationAvg;
        }
    }

    public class Regression
    {
        public decimal r;
        public decimal xAvg;
        public decimal yAvg;
        public decimal a;
        public decimal b;

        public Regression(List<Pos> posList)
        {
            decimal numerator = 0, d1 = 0, d2 = 0;
            decimal lengthList = posList.Count();

            foreach (Pos pos in posList)
            {
                xAvg += pos.timeX;
                yAvg += pos.priceY;
            }

            xAvg = xAvg / lengthList;
            yAvg = yAvg / lengthList;

            foreach (Pos pos in posList)
            {
                numerator += (pos.timeX - xAvg) * (pos.priceY - yAvg);
                d1 += (pos.timeX - xAvg) * (pos.timeX - xAvg);
                d2 += (pos.priceY - yAvg) * (pos.priceY - yAvg);
            }

            d1 = (decimal)Math.Sqrt((double)d1);
            d2 = (decimal)Math.Sqrt((double)d2);

            r = Math.Round(numerator / (d1 * d2), 4);
            decimal xSum = 0, xySum = 0, ySum = 0, xxSum = 0;

            foreach (Pos pos in posList)
            {
                xSum += pos.timeX;
                ySum += pos.priceY;
                xySum += pos.timeX * pos.priceY;
                xxSum += pos.timeX * pos.timeX;
            }

            a = (lengthList * xySum - xSum * ySum) / (lengthList * xxSum - xSum * xSum);
            b = yAvg - (a * xAvg);
        }
    }
}
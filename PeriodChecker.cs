using System;
using Binance.Net.Enums;
using Binance.Net;
using System.Collections.Generic;

namespace RegBot
{
    class PeriodChecker
    {
        public decimal bestR;
        public int bestKline;
        private int klineMin;
        private int klineMax;
        private string symbol;
        private KlineInterval interval;
        private BinanceClient client;

        public PeriodChecker(BinanceClient client, string symbol, KlineInterval interval, int klineMin, int klineMax)
        {
            this.client = client;
            this.symbol = symbol;
            this.interval = interval;
            this.klineMin = klineMin;
            this.klineMax = klineMax;
        }

        public void findBestR()
        {
            for (int limitKline = klineMin; limitKline <= klineMax; limitKline++)
            {
                List<Pos> avg = new List<Pos>();
                List<Pos> high = new List<Pos>();
                List<Pos> low = new List<Pos>();

                List<PosDv> HighExtremum = new List<PosDv>();
                List<PosDv> LowExtremum = new List<PosDv>();

                var klines = client.Spot.Market.GetKlines(symbol, interval, limit: limitKline);

                int i = 0;
                foreach (var item in klines.Data)
                {
                    if (i < limitKline - 1)
                    {
                        decimal avgPos = (item.Low + item.High) / 2;
                        decimal gapAvg = item.High - avgPos;

                        avg.Add(new Pos(i, avgPos));
                        HighExtremum.Add(new PosDv(i, item.High, gapAvg));
                        LowExtremum.Add(new PosDv(i, item.Low, gapAvg));
                        i++;
                    }
                }
                decimal[] gap = new decimal[HighExtremum.Count];
                for (int y = 0; y < HighExtremum.Count; y++)
                {
                    gap[y] = HighExtremum[y].gapAvg;
                }
                decimal median = Methods.Median(gap);
                for (int z = 0; z < HighExtremum.Count; z++)
                {
                    if (HighExtremum[z].gapAvg > median)
                    {
                        high.Add(new Pos(HighExtremum[z].timeX, HighExtremum[z].priceY));
                        low.Add(new Pos(LowExtremum[z].timeX, LowExtremum[z].priceY));
                    }
                }

                Regression Rhigh = new Regression(high);
                Regression Rlow = new Regression(low);

                decimal totalR = (Math.Abs(Rhigh.r) + Math.Abs(Rlow.r)) / 2;
                if (totalR > bestR)
                {
                    bestR = totalR;
                    bestKline = limitKline;
                }
            }

        }
    }
}


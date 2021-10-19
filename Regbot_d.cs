using System;
using Binance.Net.Enums;
using System.Collections.Generic;
using System.Linq;
using Binance.Net.Objects.Spot.MarketData;

namespace RegBot
{
    public class Regbot_d
    {
        public Configuration config;
        public decimal price;
        public decimal predictionH;
        public decimal predictionL;
        public decimal r = 0;
        public decimal rPeriod = 0;
        public int limitKline = 0;
        private PeriodChecker p;
        public string symbol;
        private Regression Rhigh;
        private Regression Rlow;
        public decimal minNotional;
        public int assetPrecision;
        public int quotePrecision;
        public decimal spread;

        public Regbot_d(Configuration config)
        {
            this.config = config;

            symbol = config.assetBase + config.assetQuote;
            IEnumerable<BinanceSymbol> symbols = config.binanceClient.client.Spot.System.GetExchangeInfo().Data.Symbols;

            decimal lotSize;
            decimal tickSize;
            int i = 0;
            while (i < symbols.Count())
            {
                if (symbols.ElementAt(i).Name == symbol)
                {
                    minNotional = symbols.ElementAt(i).MinNotionalFilter.MinNotional;
                    tickSize = symbols.ElementAt(i).PriceFilter.TickSize;
                    while ((tickSize *= 10M) <= 1M)
                    {
                        quotePrecision++;
                    }
                    lotSize = symbols.ElementAt(i).LotSizeFilter.StepSize;

                    while ((lotSize *= 10M) <= 1M)
                    {
                        assetPrecision++;
                    }
                    break;
                }
                i++;
            }
        }

        public void Run()
        {
            if (r <= rPeriod - rPeriod * config.correlationRatioLost)
            {
                p = new PeriodChecker(config.binanceClient.client, symbol, config.interval, config.minKline, config.maxKline);
                p.findBestR();
                rPeriod = p.bestR;
                limitKline = p.bestKline;
            }
            List<Pos> avg = new List<Pos>();
            List<Pos> high = new List<Pos>();
            List<Pos> low = new List<Pos>();
            List<PosDv> HighExtremum = new List<PosDv>();
            List<PosDv> LowExtremum = new List<PosDv>();

            var klines = config.binanceClient.client.Spot.Market.GetKlines(symbol, config.interval, limit: limitKline);

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

            Rhigh = new Regression(high);
            Rlow = new Regression(low);

            price = config.binanceClient.client.Spot.Market.GetPrice(symbol).Data.Price;
            r = (Math.Abs(Rhigh.r) + Math.Abs(Rlow.r)) / 2;

            predictionH = Math.Round(Rhigh.a * (limitKline + config.bonusProjection) + Rhigh.b, quotePrecision);
            predictionL = Math.Round(Rlow.a * (limitKline + config.bonusProjection) + Rlow.b, quotePrecision);

            if (predictionL > predictionH) predictionL = predictionH;
            spread = Math.Round((predictionH - predictionL) / predictionL, 5);

            new Writer("warning", "Price = " + price + " " + config.assetQuote);
            new Writer("alert", "PredictionHigh = " + predictionH + " " + config.assetQuote);
            new Writer("success", "PredictionLow = " + predictionL + " " + config.assetQuote);
            new Writer("infos", "Spread = " + spread * 100 + "%");
        }

        public decimal predictionHigh(decimal x)
        {
            if (Rhigh.a > 0)
            {
                return Math.Round(Rhigh.a * (limitKline + x + config.bonusProjection) + Rhigh.b, quotePrecision);
            }
            else
            {
                return Math.Round(Rhigh.a * (limitKline - x + config.bonusProjection) + Rhigh.b, quotePrecision);
            }
        }

        public decimal predictionLow(decimal x)
        {
            if (Rlow.a > 0)
            {
                return Math.Round(Rlow.a * (limitKline - x + config.bonusProjection) + Rlow.b, quotePrecision);
            }
            else
            {
                return Math.Round(Rlow.a * (limitKline + x + config.bonusProjection) + Rlow.b , quotePrecision);
            }
        }
    }
}
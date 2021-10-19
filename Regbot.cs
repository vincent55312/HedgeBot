using System;
using Binance.Net.Enums;
using System.Collections.Generic;
using System.Linq;
using Binance.Net.Objects.Spot.MarketData;

namespace RegBot
{
    public class Regbot
    {
        public Configuration config;
        public decimal predictionH;
        public decimal predictionL;
        public decimal spread;

        public int assetPrecision;
        public int quotePrecision;
        public decimal price;
        public decimal r = 0;
        public decimal rPeriod = 0;
        public decimal percent = 0;
        public int limitKline = 0;
        public string symbol;
        public decimal minNotional;
        private PeriodChecker p;
        private Regression regression;

        public Regbot(Configuration config)
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

        public void Run() {

            new Writer("warning", "Run on " + symbol + " " + config.interval.ToString() + " on [" + config.minKline + "-" + config.maxKline +"]");

            if (r <= rPeriod - rPeriod * config.correlationRatioLost)
            {
                p = new PeriodChecker(config.binanceClient.client, symbol, config.interval, config.minKline, config.maxKline);
                new Writer("alert", "Looking the best kline period");
                p.findBestR();
                rPeriod = p.bestR;
                limitKline = p.bestKline;   
            }
            new Writer("success", "Period : " + limitKline);
            List<Pos> avg = new List<Pos>();

            var klines = config.binanceClient.client.Spot.Market.GetKlines(symbol, config.interval, limit: limitKline);

            int i = 0;
            foreach (var item in klines.Data)
            {
                if (i < limitKline - 1)
                {
                    decimal avgPos = (item.Low + item.High) / 2;
                    avg.Add(new Pos(i, avgPos));
                    percent += (item.High - item.Low) / item.Low;
                    i++;
                }
            }

            percent = percent / (i - 1);
            regression = new Regression(avg);

            price = config.binanceClient.client.Spot.Market.GetPrice(symbol).Data.Price;
            r = regression.r;

            predictionH = predictionHighBase();
            predictionL = predictionLowBase();
            spread = Math.Round((predictionH - predictionL) / predictionL, 5);

            new Writer("warning", "Price = " + price + " " + config.assetQuote);
            new Writer("alert", "PredictionHigh = " + predictionH + " " + config.assetQuote);
            new Writer("success", "PredictionLow = " + predictionL + " " + config.assetQuote);
            new Writer("infos", "Spread = " + spread *100+"%");
        }
        public decimal predictionHigh(decimal x)
        {
            if (regression.a > 0)
            {
                return Math.Round(regression.a * (limitKline + x + config.bonusProjection) + regression.b + regression.b * percent, quotePrecision);
            }
            else
            {
                return Math.Round(regression.a * (limitKline - x + config.bonusProjection) + regression.b + regression.b * percent, quotePrecision);
            }
        }

        public decimal predictionLow(decimal x)
        {
            if (regression.a > 0)
            {
                return Math.Round(regression.a * (limitKline - x + config.bonusProjection) + regression.b + regression.b * -percent, quotePrecision);
            }
            else
            {
                return Math.Round(regression.a * (limitKline + x + config.bonusProjection) + regression.b + regression.b * -percent, quotePrecision);
            }
        }

        private decimal predictionHighBase()
        {
            if (regression.a > 0)
            {
                return Math.Round(regression.a * (limitKline + config.bonusProjection) + regression.b + regression.b * percent, quotePrecision);
            }
            else
            {
                return Math.Round(regression.a * (limitKline + config.bonusProjection) + regression.b + regression.b * percent, quotePrecision);
            }
        }

        private decimal predictionLowBase()
        {
            if (regression.a > 0)
            {
                return Math.Round(regression.a * (limitKline + config.bonusProjection) + regression.b + regression.b * -percent, quotePrecision);
            }
            else
            {
                return Math.Round(regression.a * (limitKline + config.bonusProjection) + regression.b + regression.b * -percent, quotePrecision);
            }
        }
    }
}
       
using Binance.Net.Enums;

namespace RegBot
{
    public class Configuration
    {
        public string idBot;
        public Client binanceClient;

        public string assetBase;
        public string assetQuote;
        public KlineInterval interval;
        public int minKline;
        public int maxKline;
        public decimal correlationRatioLost;
        public decimal bonusProjection;

        public Configuration(string idBot, string assetBase, string assetQuote, int idInterval, int minKline, int maxKline,
            decimal correlationRatioLost, decimal bonusProjection)
        {
            this.idBot = idBot;
            binanceClient = new Client("HLyEMvA6tOAbVdkXQlOpLZSAn6iMwhtIgHtKwc59ZLxMJhhn8KIkhSG9SyyUg3aF", "bh6IsOZRGbGxdGu3TZa7uzXG4ZwZwZz4hizCNylBRIWkg1HrFavsJXjevyWsIjDd");
            this.assetBase = assetBase.ToUpper();
            this.assetQuote = assetQuote.ToUpper();
            this.interval = getKlineInterval(idInterval);
            this.minKline = minKline;
            this.maxKline = maxKline;
            this.correlationRatioLost = correlationRatioLost;
            this.bonusProjection = bonusProjection;
        }

        private KlineInterval getKlineInterval(int id)
        {
            switch (id)
            {
                case 1:
                    return KlineInterval.OneMinute;
                case 2:
                    return KlineInterval.ThreeMinutes;
                case 3:
                    return KlineInterval.FiveMinutes;
                case 4:
                    return KlineInterval.FifteenMinutes;
                case 5:
                    return KlineInterval.ThirtyMinutes;
                case 6:
                    return KlineInterval.OneHour;
                case 7:
                    return KlineInterval.TwoHour;
                case 8:
                    return KlineInterval.FourHour;
                case 9:
                    return KlineInterval.SixHour;
                case 10:
                    return KlineInterval.EightHour;
                case 11:
                    return KlineInterval.TwelveHour;
                case 12:
                    return KlineInterval.OneDay;
                default:
                    return KlineInterval.FiveMinutes;
            }
        }
    }
}

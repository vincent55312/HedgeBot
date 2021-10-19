using System;
using System.Collections.Generic;
using System.Linq;
using Binance.Net.Objects.Spot.MarketData;
using Binance.Net.Objects.Spot.SpotData;
using CryptoExchange.Net.Objects;

namespace RegBot
{
    public class BinanceTrading
    {
        public Regbot regbot;

        private decimal nBnbAsset;
        private int bnbAssetPrecision;
        private int bnbQuotePrecision;
        private string bnbQuote;
        private decimal bnbMinNotional;
        private decimal nBaseAsset;
        private decimal nQuoteAsset;

        public BinanceTrading(Regbot regbot)
        {
            this.regbot = regbot;

            IEnumerable<BinanceSymbol> symbols = regbot.config.binanceClient.client.Spot.System.GetExchangeInfo().Data.Symbols;
            bnbQuote = "BNB" + regbot.config.assetQuote;

            int i = 0;
            while (i < symbols.Count())
            {
                if (symbols.ElementAt(i).Name == bnbQuote)
                {
                    decimal tickSize = symbols.ElementAt(i).PriceFilter.TickSize;
                    decimal lotSize = symbols.ElementAt(i).LotSizeFilter.StepSize;
                    bnbMinNotional = symbols.ElementAt(i).MinNotionalFilter.MinNotional;
                    while ((lotSize *= 10M) <= 1M)
                    {
                        bnbAssetPrecision++;
                    }
                    while ((tickSize *= 10M) <= 1M)
                    {
                        bnbQuotePrecision++;
                    }
                    break;
                }
                i++;
            }
            bnbMinNotional = bnbMinNotional + bnbMinNotional * 0.1M;
        }

        public void Run()
        {
            if (regbot.config.binanceClient.CheckCredentials())
            {
                regbot.Run();
                cancelAllOpenOrders();
                if (regbot.config.autoBuyBNB) purchaseBnb();
                updateBalances();

                if (regbot.config.nQuoteAssetMax != 0)
                {
                    decimal nBaseAssetMax = regbot.config.nQuoteAssetMax / regbot.price;
                    if (nBaseAsset > nBaseAssetMax) nBaseAsset = nBaseAssetMax;
                    if (nQuoteAsset > regbot.config.nQuoteAssetMax) nQuoteAsset = regbot.config.nQuoteAssetMax + regbot.config.nQuoteAssetMax *2/100;
                }

                int nOrdersBuy = (int)Math.Round(nQuoteAsset / regbot.minNotional, 0, MidpointRounding.ToZero);
                int nOrdersSell = (int)Math.Round(nBaseAsset / (regbot.minNotional / regbot.price), 0, MidpointRounding.ToZero);

                if (nOrdersBuy > regbot.config.maxOrders) nOrdersBuy = regbot.config.maxOrders;
                if (nOrdersSell > regbot.config.maxOrders) nOrdersSell = regbot.config.maxOrders;

                decimal buyQuantity = 0;
                decimal sellQuantity = 0;

                if (nOrdersBuy > 0) buyQuantity = Math.Round(nQuoteAsset / nOrdersBuy, regbot.quotePrecision, MidpointRounding.ToZero);
                if (nOrdersSell > 0) sellQuantity = Math.Round(nBaseAsset / nOrdersSell, regbot.assetPrecision, MidpointRounding.ToZero);

                new Writer("warning", "nOrdersSell : " + nOrdersSell);
                new Writer("warning", "nOrdersBuy : " + nOrdersBuy);

                int i = 0;
                while (i < nOrdersSell)
                {
                    if (regbot.predictionHigh(i) < regbot.price)
                    {
                        nOrdersSell++;
                        new Writer("alert", "skip sell");
                    }
                    else
                    {
                        limitOrderSell(regbot.predictionHigh(i), sellQuantity);
                    }
                    i++;
                }

                i = 0;
                while (i < nOrdersBuy)
                {
                    if (regbot.predictionLow(i) > regbot.price)
                    {
                        nOrdersBuy++;
                        new Writer("alert", "skip buy");
                    }
                    else
                    {
                        limitOrderBuy(regbot.predictionLow(i), Math.Round(buyQuantity / regbot.predictionLow(i), regbot.assetPrecision, MidpointRounding.ToZero));
                    }
                    i++;
                }
                new Sql().updateTrades(regbot.config.binanceClient);
                new Sql().addInfos(regbot);
                new Writer("success", "Balances amounts : " + nBaseAsset + regbot.config.assetBase + " " + nQuoteAsset + regbot.config.assetQuote + "\n");
            }
        }

        private void cancelAllOpenOrders()
        {
            bool cancelOrders = regbot.config.binanceClient.client.Spot.Order.CancelAllOpenOrders(regbot.symbol).Success;
        }

        private void limitOrderBuy(decimal price, decimal quantity)
        {
            WebCallResult<BinancePlacedOrder> limitBuy = regbot.config.binanceClient.client.Spot.Order.PlaceOrder(
                regbot.symbol,
                Binance.Net.Enums.OrderSide.Buy,
                Binance.Net.Enums.OrderType.Limit,
                quantity,
                null,
                null,
                price,
                Binance.Net.Enums.TimeInForce.GoodTillCancel);

            if (limitBuy.Success)
            {
                new Sql().addTrade(limitBuy, regbot.config);
                new Writer("success", "Buy order : price : " + price + " quantity : " + quantity);
            }
            else
            {
                new Writer("alert", "Buy order failed : price : " + price + " quantity : " + quantity);
            }
        }

        private void limitOrderSell(decimal price, decimal quantity)
        {
            WebCallResult<BinancePlacedOrder> limitSell = regbot.config.binanceClient.client.Spot.Order.PlaceOrder(
                regbot.symbol,
                Binance.Net.Enums.OrderSide.Sell,
                Binance.Net.Enums.OrderType.Limit,
                quantity,
                null,
                null,
                price,
                Binance.Net.Enums.TimeInForce.GoodTillCancel);

            if (limitSell.Success)
            {
                new Sql().addTrade(limitSell, regbot.config);
                new Writer("hint", "Sell order : price : " + price + " quantity : " + quantity);
            }
            else
            {
                new Writer("alert", "Sell order failed : price : " + price + " quantity : " + quantity);
            }
        }

        private void updateBalances()
        {
            IEnumerable<BinanceBalance> balances = regbot.config.binanceClient.client.General.GetAccountInfo().Data.Balances;

            int i = 0;
            while (i < balances.Count())
            {
                if (balances.ElementAt(i).Asset == regbot.config.assetBase)
                {
                    nBaseAsset = balances.ElementAt(i).Free;
                    break;
                }
                i++;
            }

            i = 0;
            while (i < balances.Count())
            {
                if (balances.ElementAt(i).Asset == regbot.config.assetQuote)
                {
                    nQuoteAsset = balances.ElementAt(i).Free;
                    break;
                }
                i++;
            }
        }

        private void purchaseBnb()
        {
            bool cancelOrders = regbot.config.binanceClient.client.Spot.Order.CancelAllOpenOrders(bnbQuote).Success;
            decimal priceBnb = regbot.config.binanceClient.client.Spot.Market.GetPrice(bnbQuote).Data.Price;

            IEnumerable<BinanceBalance> balances = regbot.config.binanceClient.client.General.GetAccountInfo().Data.Balances;
            int i = 0;
            while (i < balances.Count())
            {
                if (balances.ElementAt(i).Asset == "BNB")
                {
                    nBnbAsset = balances.ElementAt(i).Free;
                    break;
                }
                i++;
            }

            if (nBnbAsset < bnbMinNotional / priceBnb)
            {
                new Writer("alert", "Need to purchase BNB");
                bool limitBuy = regbot.config.binanceClient.client.Spot.Order.PlaceOrder(
                    bnbQuote,
                    Binance.Net.Enums.OrderSide.Buy,
                    Binance.Net.Enums.OrderType.Limit,
                    Math.Round(bnbMinNotional / priceBnb, bnbAssetPrecision, MidpointRounding.ToZero),
                    null,
                    null,
                    Math.Round(priceBnb * 0.999M, bnbQuotePrecision, MidpointRounding.ToZero),
                    Binance.Net.Enums.TimeInForce.GoodTillCancel).Success;
            }
        }
    }
}
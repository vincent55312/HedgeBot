using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RegBot
{
    class Simulation_d
    {
        private Regbot_d regbot;
        private bool isBuyer;
        private decimal totAssetQuote;
        private decimal totAssetBase;
        private decimal ratioFees = 0.00075M;

        public Simulation_d(Regbot_d regbot)
        {
            this.regbot = regbot;
            isBuyer = true;
            totAssetBase = 0;
            totAssetQuote = 10000;
        }

        public void Run()
        {
            regbot.Run();
            if (isBuyer)
            {
                if (regbot.price <= regbot.predictionL)
                {
                    isBuyer = false;
                    decimal fees = totAssetQuote * ratioFees;
                    totAssetBase = (totAssetQuote - fees) / regbot.price;
                    totAssetQuote = 0;
                    string msg = "id : " + regbot.config.idBot + " BUY position at " + regbot.price +
                        regbot.config.assetQuote + " for " + totAssetBase + regbot.config.assetBase + " fees : " + fees + regbot.config.assetQuote;
                    new Log(msg);
                    new Writer("warning", msg);
                }
                else
                {
                    new Writer("infos", regbot.config.idBot + " NEED BUY AT : " + regbot.predictionL + " Price : " + regbot.price);
                }
            }
            else
            {
                if (regbot.price >= regbot.predictionH)
                {
                    isBuyer = true;
                    decimal fees = totAssetBase * ratioFees;
                    totAssetQuote = (totAssetBase - fees) * regbot.price;
                    totAssetBase = 0;
                    string msg = "id : " + regbot.config.idBot + " SOLD position at " + regbot.price + regbot.config.assetQuote +
                        "for " + totAssetQuote + regbot.config.assetQuote + " fees : " + fees + regbot.config.assetBase;
                    new Log(msg);
                    new Writer("warning", msg);
                }
                else
                {
                    new Writer("infos", regbot.config.idBot + " NEED SELL AT : " + regbot.predictionH + " Price : " + regbot.price);
                }
            }
        }
    }
}
using Binance.Net;
using CryptoExchange.Net.Authentication;
using Binance.Net.Objects.Spot;

namespace RegBot
{
    public class Client
    {
        public BinanceClient client;
        public string apikey;

        public Client(string apikey, string secretkey)
        {
            this.apikey = apikey;
            BinanceClient.SetDefaultOptions(
            new BinanceClientOptions
            {
                ApiCredentials = new ApiCredentials(apikey, secretkey)
            }
            );
            client = new BinanceClient();
        }

        public bool CheckCredentials()
        {
            if (client.General.GetAccountInfo().Success) return true;
            else return false;
        }
    }
}

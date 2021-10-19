using System;
using System.Collections.Generic;
using Binance.Net.Objects.Spot.SpotData;
using CryptoExchange.Net.Objects;
using MySql.Data.MySqlClient;
using Binance.Net.Enums;

namespace RegBot
{
    public class Sql
    {
        string cs = @"server=localhost;userid=hedge;password=;database=hedgetrade";
        public Dictionary<string, BinanceTrading> instances = new Dictionary<string, BinanceTrading>();
        public Dictionary<string, BinanceTrading_d> instances_d = new Dictionary<string, BinanceTrading_d>();

        public void reload()
        {
            new Writer("infos", "Reload...");
            var connection = new MySqlConnection(cs);
            connection.Open();
            string sql = "SELECT * FROM config WHERE reload = 1 AND apikey != 'nan' AND secretkey != 'nan'";
            using var cmd = new MySqlCommand(sql, connection);
            using MySqlDataReader rdr = cmd.ExecuteReader();

            if (rdr.HasRows)
            {
                while (rdr.Read())
                {
                    DateTime dateExpiration = DateTime.Parse(rdr.GetString(3));

                    if (DateTime.Now <= dateExpiration)
                    {
                        string idBot = rdr.GetString(0);
                        try { instances.Remove(idBot); } catch { }
                        try { instances_d.Remove(idBot); } catch { }
                        string apikey = rdr.GetString(4);
                        string secretkey = rdr.GetString(5);
                        string assetBase = rdr.GetString(6);
                        string assetQuote = rdr.GetString(7);
                        int activated = rdr.GetInt16(17);
                        int double_regression = rdr.GetInt16(18);

                        if (new Client(apikey, secretkey).CheckCredentials() == true && activated == 1)
                        {
                            int idInterval = rdr.GetInt16(8);
                            int minKline = rdr.GetInt16(9);
                            int maxKline = rdr.GetInt16(10);
                            decimal correlationRatioLost = rdr.GetDecimal(11);
                            decimal multiplicatorRatio = rdr.GetDecimal(12);
                            decimal bonusProjection = rdr.GetDecimal(13);
                            int maxOrders = rdr.GetInt16(14);
                            int autoBuyBnb = rdr.GetInt16(15);
                            decimal nQuoteMaxAsset = rdr.GetDecimal(16);

                            if (double_regression == 0)
                            {
                                instances.Add(idBot, new BinanceTrading(new Regbot(new Configuration(idBot, apikey, secretkey, assetBase, assetQuote, idInterval,
                                minKline, maxKline, correlationRatioLost, multiplicatorRatio, maxOrders, autoBuyBnb, nQuoteMaxAsset, bonusProjection))));
                            }
                            else
                            {
                                instances_d.Add(idBot, new BinanceTrading_d(new Regbot_d(new Configuration(idBot, apikey, secretkey, assetBase, assetQuote, idInterval,
                                minKline, maxKline, correlationRatioLost, multiplicatorRatio, maxOrders, autoBuyBnb, nQuoteMaxAsset, bonusProjection))));
                            }


                            new Writer("success", "Instance reloaded : " + idBot + "\n");

                            var connectionUpdate = new MySqlConnection(cs);
                            connectionUpdate.Open();
                            using var command = new MySqlCommand();
                            command.Connection = connectionUpdate;
                            command.CommandText = "UPDATE config SET reload = 0 WHERE id_bot = '" + idBot + "'";
                            command.ExecuteNonQuery();
                            connectionUpdate.Close();
                        }
                    }
                }
            }
            new Writer("infos", "End Reload");
            connection.Close();
            connection = null;
        }
        public void loadAllConfig()
        {
            instances.Clear();
            instances_d.Clear();
            new Writer("infos", "Loading all configurations...");

            var connection = new MySqlConnection(cs);
            connection.Open();
            string sql = "SELECT * FROM config WHERE activated = 1";

            using var cmd = new MySqlCommand(sql, connection);
            using MySqlDataReader rdr = cmd.ExecuteReader();
            int nConfig = 0;

            while (rdr.Read())
            {
                DateTime dateExpiration = DateTime.Parse(rdr.GetString(3));
                if (DateTime.Now <= dateExpiration)
                {
                    string idBot = rdr.GetString(0);
                    string apikey = rdr.GetString(4);
                    string secretkey = rdr.GetString(5);
                    string assetBase = rdr.GetString(6);
                    string assetQuote = rdr.GetString(7);
                    int activated = rdr.GetInt16(17);

                    if (new Client(apikey, secretkey).CheckCredentials() == true && activated == 1)
                    {
                        int idInterval = rdr.GetInt16(8);
                        int minKline = rdr.GetInt16(9);
                        int maxKline = rdr.GetInt16(10);
                        decimal correlationRatioLost = rdr.GetDecimal(11);
                        decimal multiplicatorRatio = rdr.GetDecimal(12);
                        decimal bonusProjection = rdr.GetDecimal(13);
                        int maxOrders = rdr.GetInt16(14);
                        int autoBuyBnb = rdr.GetInt16(15);
                        decimal nQuoteMaxAsset = rdr.GetDecimal(16);
                        int double_regression = rdr.GetInt16(18);

                        if (double_regression == 0)
                        {
                            instances.Add(idBot, new BinanceTrading(new Regbot(new Configuration(idBot, apikey, secretkey, assetBase, assetQuote, idInterval,
                            minKline, maxKline, correlationRatioLost, multiplicatorRatio, maxOrders, autoBuyBnb, nQuoteMaxAsset, bonusProjection))));
                        }
                        else
                        {
                            instances_d.Add(idBot, new BinanceTrading_d(new Regbot_d(new Configuration(idBot, apikey, secretkey, assetBase, assetQuote, idInterval,
                            minKline, maxKline, correlationRatioLost, multiplicatorRatio, maxOrders, autoBuyBnb, nQuoteMaxAsset, bonusProjection))));
                        }

                        nConfig++;
                        new Writer("success", "Instance loaded : " + idBot);
                    }
                }
            }
            new Writer("success", "Total instances loaded : " + nConfig + "\n");
            connection.Close();
        }

        public void addTrade(WebCallResult<BinancePlacedOrder> order, Configuration config)
        {
            var connection = new MySqlConnection(cs);
            connection.Open();

            var sql = "INSERT INTO trades (id_trade, id_bot, asset_base, asset_quote, side, quantity, price, time, status, quantity_filled) " +
                "VALUES(@id_trade, @id_bot, @asset_base, @asset_quote, @side, @quantity, @price, @time, @status, @quantity_filled)";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id_trade", order.Data.OrderId.ToString());
            command.Parameters.AddWithValue("@id_bot", config.idBot);
            command.Parameters.AddWithValue("@asset_base", config.assetBase);
            command.Parameters.AddWithValue("@asset_quote", config.assetQuote);
            command.Parameters.AddWithValue("@side", order.Data.Side.ToString());
            command.Parameters.AddWithValue("@quantity", order.Data.Quantity);
            command.Parameters.AddWithValue("@price", order.Data.Price);
            command.Parameters.AddWithValue("@time", order.Data.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@status", order.Data.Status.ToString());
            command.Parameters.AddWithValue("@quantity_filled", order.Data.QuantityFilled);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public void updateTrades(Client binanceClient)
        {
            var connection = new MySqlConnection(cs);
            var connectionUpdate = new MySqlConnection(cs);

            connection.Open();
            string sql = "SELECT * FROM trades WHERE status = 'New' OR status = 'PartiallyFilled'";
            using var cmd = new MySqlCommand(sql, connection);
            using MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                try
                {
                    string orderId = rdr.GetString(0);
                    string assetBase = rdr.GetString(2);
                    string assetQuote = rdr.GetString(3);

                    BinanceOrder order = binanceClient.client.Spot.Order.GetOrder(assetBase + assetQuote, long.Parse(orderId)).Data;
                    OrderStatus status = order.Status;
                    connectionUpdate.Open();

                    if (status == OrderStatus.Canceled)
                    {
                        using var command = new MySqlCommand();
                        command.Connection = connectionUpdate;
                        command.CommandText = "DELETE FROM trades WHERE id_trade = " + orderId;
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        using var command = new MySqlCommand(sql, connectionUpdate);
                        command.CommandText = "UPDATE trades SET status = @status , quantity_filled = @quantity_filled WHERE id_trade = " + orderId;

                        command.Parameters.AddWithValue("@status", order.Status.ToString());
                        command.Parameters.AddWithValue("@quantity_filled", order.QuantityFilled);
                        command.ExecuteNonQuery();
                    }
                    connectionUpdate.Close();
                }
                catch
                {
                    connectionUpdate.Close();
                }
            }
            connection.Close();
        }

        public void addInfos(Regbot regbot)
        {
            var connection = new MySqlConnection(cs);
            connection.Open();
            string sql = "SELECT * FROM infos WHERE id_bot = '" + regbot.config.idBot + "'";
            using var cmd = new MySqlCommand(sql, connection);
            using MySqlDataReader rdr = cmd.ExecuteReader();

            if (rdr.HasRows)
            {
                var connectionUpdate = new MySqlConnection(cs);
                connectionUpdate.Open();
                using var command = new MySqlCommand(sql, connectionUpdate);
                command.CommandText = "UPDATE infos SET prediction_high = @prediction_high, prediction_low = @prediction_low, period = @period, spread = @spread, price = @price, symbol = @symbol, last_update = @last_update WHERE id_bot = '" + regbot.config.idBot + "'";

                command.Parameters.AddWithValue("@prediction_high", regbot.predictionH);
                command.Parameters.AddWithValue("@prediction_low", regbot.predictionL);
                command.Parameters.AddWithValue("@period", regbot.limitKline);
                command.Parameters.AddWithValue("@spread", regbot.spread);
                command.Parameters.AddWithValue("@price", regbot.price);
                command.Parameters.AddWithValue("@symbol", regbot.symbol);
                command.Parameters.AddWithValue("@last_update", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

                command.ExecuteNonQuery();
                connectionUpdate.Close();
            }
            else
            {
                var connectionInsert = new MySqlConnection(cs);
                connectionInsert.Open();

                var sqlInsert = "INSERT INTO infos (id_bot, prediction_high, prediction_low, period, spread, price, symbol, last_update) " +
                    "VALUES(@id_bot, @prediction_high, @prediction_low, @period, @spread, @price, @symbol, @last_update)";

                using var command = new MySqlCommand(sqlInsert, connectionInsert);

                command.Parameters.AddWithValue("@id_bot", regbot.config.idBot);
                command.Parameters.AddWithValue("@prediction_high", regbot.predictionH);
                command.Parameters.AddWithValue("@prediction_low", regbot.predictionL);
                command.Parameters.AddWithValue("@period", regbot.limitKline);
                command.Parameters.AddWithValue("@spread", regbot.spread);
                command.Parameters.AddWithValue("@price", regbot.price);
                command.Parameters.AddWithValue("@symbol", regbot.symbol);
                command.Parameters.AddWithValue("@last_update", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

                command.ExecuteNonQuery();
                connectionInsert.Close();
            }
            connection.Close();
        }

        public void addInfos(Regbot_d regbot)
        {
            var connection = new MySqlConnection(cs);
            connection.Open();
            string sql = "SELECT * FROM infos WHERE id_bot = '" + regbot.config.idBot + "'";
            using var cmd = new MySqlCommand(sql, connection);
            using MySqlDataReader rdr = cmd.ExecuteReader();

            if (rdr.HasRows)
            {
                var connectionUpdate = new MySqlConnection(cs);
                connectionUpdate.Open();
                using var command = new MySqlCommand(sql, connectionUpdate);
                command.CommandText = "UPDATE infos SET prediction_high = @prediction_high, prediction_low = @prediction_low, period = @period, spread = @spread, price = @price, symbol = @symbol, last_update = @last_update WHERE id_bot = '" + regbot.config.idBot + "'";

                command.Parameters.AddWithValue("@prediction_high", regbot.predictionH);
                command.Parameters.AddWithValue("@prediction_low", regbot.predictionL);
                command.Parameters.AddWithValue("@period", regbot.limitKline);
                command.Parameters.AddWithValue("@spread", regbot.spread);
                command.Parameters.AddWithValue("@price", regbot.price);
                command.Parameters.AddWithValue("@symbol", regbot.symbol);
                command.Parameters.AddWithValue("@last_update", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

                command.ExecuteNonQuery();
                connectionUpdate.Close();
            }
            else
            {
                var connectionInsert = new MySqlConnection(cs);
                connectionInsert.Open();

                var sqlInsert = "INSERT INTO infos (id_bot, prediction_high, prediction_low, period, spread, price, symbol, last_update) " +
                    "VALUES(@id_bot, @prediction_high, @prediction_low, @period, @spread, @price, @symbol, @last_update)";

                using var command = new MySqlCommand(sqlInsert, connectionInsert);

                command.Parameters.AddWithValue("@id_bot", regbot.config.idBot);
                command.Parameters.AddWithValue("@prediction_high", regbot.predictionH);
                command.Parameters.AddWithValue("@prediction_low", regbot.predictionL);
                command.Parameters.AddWithValue("@period", regbot.limitKline);
                command.Parameters.AddWithValue("@spread", regbot.spread);
                command.Parameters.AddWithValue("@price", regbot.price);
                command.Parameters.AddWithValue("@symbol", regbot.symbol);
                command.Parameters.AddWithValue("@last_update", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

                command.ExecuteNonQuery();
                connectionInsert.Close();
            }
            connection.Close();
        }
    }
}

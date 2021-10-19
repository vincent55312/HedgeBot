using System;
using System.Threading;
using System.Collections.Generic;

namespace RegBot
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Simulation> std = new List<Simulation>();
            List<Simulation_d> older = new List<Simulation_d>();

            std.Add(new Simulation(new Regbot(new Configuration("1s", "BTC", "USDT", 3, 40, 80, 0.01M, 5))));
            std.Add(new Simulation(new Regbot(new Configuration("2s", "BTC", "USDT", 3, 80, 160, 0.01M, 5))));
            std.Add(new Simulation(new Regbot(new Configuration("3s", "BTC", "USDT", 3, 40, 80, 0.05M, 5))));
            std.Add(new Simulation(new Regbot(new Configuration("4s", "BTC", "USDT", 3, 80, 160, 0.05M, 5))));

            older.Add(new Simulation_d(new Regbot_d(new Configuration("1o", "BTC", "USDT", 3, 40, 80, 0.01M, 5))));
            older.Add(new Simulation_d(new Regbot_d(new Configuration("2o", "BTC", "USDT", 3, 80, 160, 0.01M, 5))));
            older.Add(new Simulation_d(new Regbot_d(new Configuration("3o", "BTC", "USDT", 3, 40, 80, 0.05M, 5))));
            older.Add(new Simulation_d(new Regbot_d(new Configuration("4o", "BTC", "USDT", 3, 80, 160, 0.05M, 5))));

            while (true)
            {
                foreach(var item in std)
                {
                    item.Run();
                    Thread.Sleep(20 * 1000);
                }
                foreach (var item in older)
                {
                    item.Run();
                    Thread.Sleep(20 * 1000);
                }
            }
        }
    }
}
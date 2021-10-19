using System;
using System.Threading;


namespace RegBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Sql hedgetrade = new Sql();
            hedgetrade.loadAllConfig();
            int i = 0;

            while (true)
            {
                foreach (var instance in hedgetrade.instances)
                {
                    new Writer("success", "Run on bot id :" + instance.Value.regbot.config.idBot);

                    instance.Value.Run();
                    Thread.Sleep(1 * 60 * 1000);
                }

                foreach (var instance in hedgetrade.instances_d)
                {
                    new Writer("success", "Run on bot id :" + instance.Value.regbot.config.idBot);

                    instance.Value.Run();
                    Thread.Sleep(1 * 60 * 1000);
                }
                
                hedgetrade.reload();

                i += (5 * 60 * 1000);
                if (i >= 60 * 1000 * 60 * 24)
                {
                    i = 0;
                    hedgetrade.loadAllConfig();
                }
            }
        }
    }
}
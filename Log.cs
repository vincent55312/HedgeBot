using System;
using System.IO;
using System.Reflection;

namespace RegBot
{
    class Log
    {
        private string pathLog { get; set; }
        public Log(string message)
        {
            Write(message);
        }

        public void Write(string message)
        {
            pathLog = Directory.GetCurrentDirectory() + "/log.txt";

            try
            {
                using (StreamWriter writer = File.AppendText(pathLog))
                {
                    writer.WriteLine(DateTime.UtcNow.ToString() + " " + message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
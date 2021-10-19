using System;

namespace RegBot
{
    class Writer
    {
        private ConsoleColor consoleColor;
        private ConsoleColor defaultColor = ConsoleColor.White;
        public Writer(string typeColor, string message)
        {
            switch (typeColor)
            {
                case "alert":
                    consoleColor = ConsoleColor.Red;
                    break;
                case "warning":
                    consoleColor = ConsoleColor.Yellow;
                    break;
                case "success":
                    consoleColor = ConsoleColor.Green;
                    break;
                case "infos":
                    consoleColor = ConsoleColor.Cyan;
                    break;
                case "hint":
                    consoleColor = ConsoleColor.Gray;
                    break;
                default:
                    consoleColor = defaultColor;
                    break;
            }
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(getDate() + " -> " + message);
            Console.ForegroundColor = defaultColor;
        }

        public string getDate()
        {
            return DateTime.UtcNow.ToLongTimeString();
        }
    }

}
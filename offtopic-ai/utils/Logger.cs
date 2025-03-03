using System;
using System.IO;

namespace offtopic_ai.utils
{
    public static class Logger
    {
        private static readonly string LogFilePath = "logs.txt";
        private static readonly object _lock = new object();

        public const string Info = "Info";
        public const string Warning = "Warning";
        public const string Error = "Error";

        public static void Log(string level, string message)
        {
            string symbol = GetLogSymbol(level);
            ConsoleColor symbolColor = GetLogColor(level);

            lock (_lock) 
            {
                Console.Write("[");  
                Console.ForegroundColor = symbolColor;
                Console.Write(symbol);
                Console.ResetColor();
                Console.Write("] - ");
                Console.WriteLine(message);
            }
            
            lock (_lock)
            {
                File.AppendAllText(LogFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [ {symbol} ] - {message}{Environment.NewLine}");
            }
        }

        private static string GetLogSymbol(string level)
        {
            return level switch
            {
                Info => "+",
                Warning => "?",
                Error => "!",
                _ => "?"
            };
        }

        private static ConsoleColor GetLogColor(string level)
        {
            return level switch
            {
                Info => ConsoleColor.Cyan, 
                Warning => ConsoleColor.Yellow,
                Error => ConsoleColor.Red,
                _ => ConsoleColor.White
            };
        }
    }
}
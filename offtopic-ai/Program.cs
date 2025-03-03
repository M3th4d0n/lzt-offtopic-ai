using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using offtopic_ai.utils;
using offtopic_ai;

class Program
{
    static async Task Main()
    {
        Logger.Log(Logger.Info, "Запуск мониторинга новых тем на форуме");
        ForumMonitor monitor = new ForumMonitor();
        await monitor.StartMonitoring();
    }
}
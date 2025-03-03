using System.Text.Json;
using offtopic_ai.utils;
using offtopic_ai.Models;
using offtopic_ai;
using Thread = offtopic_ai.Models.Thread;

namespace offtopic_ai;

class ForumMonitor
{
    private static readonly int forumId = 8;
    private static readonly int delayBetweenRequests = 3000;
    private static readonly int checkInterval = 15000;
    private static readonly int maxThreadAgeMinutes = 5; 
    private ThreadFetcher fetcher = new ThreadFetcher();
    private AI ai = new AI();
    private LolzPoster poster = new LolzPoster();
    private HashSet<int> answeredThreads = new HashSet<int>();
    private const string jsonFilePath = "answered_threads.json";

    public ForumMonitor()
    {
        LoadAnsweredThreads();
    }

    public async Task StartMonitoring()
    {
        while (true)
        {
            try
            {
                await ProcessNewThreads();
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.Error, "Ошибка: " + ex.Message);
            }
            await Task.Delay(checkInterval);
        }
    }

    private async Task ProcessNewThreads()
{
    List<Thread> threads = await fetcher.GetThreads(forumId);
    if (threads == null) return;

    foreach (var thread in threads)
    {
        if (thread.creator_username == "BMWM5")
        {
            Logger.Log(Logger.Warning, "skiped admin theme");
            answeredThreads.Add(thread.thread_id);
            SaveAnsweredThreads();
            continue;
        }

        var threadCreationTime = DateTimeOffset.FromUnixTimeSeconds(thread.thread_create_date).UtcDateTime;
        var ageMinutes = (DateTime.UtcNow - threadCreationTime).TotalMinutes;

        if (ageMinutes > maxThreadAgeMinutes)
            continue;

        await Task.Delay(delayBetweenRequests);
        var (title, content, hasMedia) = await fetcher.GetThreadDetails(thread.thread_id);
        
        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
            continue;
        
        //if (hasMedia)
        //{
        //    Logger.Log(Logger.Warning, $"Пропущена тема '{title}' (содержит медиа)");
        //    continue;
        //}

        string prompt = $"{title}\n\n{content}";

        Logger.Log(Logger.Info, "========================================");
        Logger.Log(Logger.Info, $"Новая тема: {title}");
        Logger.Log(Logger.Info, $"Author: {thread.creator_username}");
        Logger.Log(Logger.Info, $"y.o {ageMinutes:F2}");
        Logger.Log(Logger.Info, $"Ссылка: {thread.links.permalink}");
        Logger.Log(Logger.Info, $"Содержание:{content}");
        Logger.Log(Logger.Info, "========================================");
        Logger.Log(Logger.Info, "Ожидание ответа от AI...");
        string aiResponse = await ai.GetAIResponse(prompt);

        Logger.Log(Logger.Info, $"Ответ AI:{aiResponse}");
        
        
        bool success = await poster.PostReply(thread.thread_id, $"{aiResponse} :yodaluv:");
        if (success)
        {
            Logger.Log(Logger.Info, $"Ответ успешно отправлен в тему: {thread.thread_title}");
            answeredThreads.Add(thread.thread_id);
            SaveAnsweredThreads();
        }
        else
        {
            Logger.Log(Logger.Error, "Ошибка при отправке ответа в тему.");
        }

        await Task.Delay(delayBetweenRequests);
    }
}


    private void LoadAnsweredThreads()
    {
        if (File.Exists(jsonFilePath))
        {
            try
            {
                string json = File.ReadAllText(jsonFilePath);
                var threadIds = JsonSerializer.Deserialize<HashSet<int>>(json);
                if (threadIds != null)
                {
                    answeredThreads = threadIds;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.Error, "Ошибка загрузки JSON: " + ex.Message);
            }
        }
    }

    private void SaveAnsweredThreads()
    {
        try
        {
            string json = JsonSerializer.Serialize(answeredThreads, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(jsonFilePath, json);
        }
        catch (Exception ex)
        {
            Logger.Log(Logger.Error, "Ошибка сохранения JSON: " + ex.Message);
        }
    }
}

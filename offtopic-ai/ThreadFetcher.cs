
using offtopic_ai.Models;
using System.Text.Json;
using offtopic_ai.utils;
using offtopic_ai;
using Thread = offtopic_ai.Models.Thread;

namespace offtopic_ai;

class ThreadFetcher
{
    private static readonly HttpClient client = new HttpClient();
    private const string apiUrl = "https://api.zelenka.guru/threads";
    private const string apiKey = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzUxMiJ9.eyJzdWIiOjc0MjQxNzIsImlzcyI6Imx6dCIsImV4cCI6MCwiaWF0IjoxNzQwNzcxNjY4LCJqdGkiOjc0NTg5Nywic2NvcGUiOiJiYXNpYyByZWFkIHBvc3QgY29udmVyc2F0ZSBtYXJrZXQifQ.uHddk-IzaFDVUGZI6-eY16N_jByzKVZQmHFI0JA2a1Wsqc4AlEN5uw8jakic85yPezACocnLc72aUvALzCt84Lbu_mXIWIobo5HdEZECVPTktzP2cNPdP3-629f06-Q1E3Ez6N5egrhNMUoEPFKbjsgsK3YDEWLfif7hb75uzSA";

    public async Task<List<Thread>> GetThreads(int forumId)
    {
        var url = $"{apiUrl}?forum_id={forumId}&limit=10&order=desc";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Authorization", $"Bearer {apiKey}");

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            Logger.Log(Logger.Error, $"Ошибка запроса: {response.StatusCode}");
            return null;
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var threads = JsonSerializer.Deserialize<ThreadListResponse>(jsonResponse);
        return threads?.threads;
    }

    public async Task<(string Title, string Content, bool HasMedia)> GetThreadDetails(int threadId)
    {
        var url = $"https://api.zelenka.guru/threads/{threadId}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Authorization", $"Bearer {apiKey}");

        var response = await client.SendAsync(request);
        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            Logger.Log(Logger.Warning, "Превышен лимит запросов. Ожидание перед повторным запросом...");
            await Task.Delay(5000);
            return (null, null, false);
        }

        if (!response.IsSuccessStatusCode)
        {
            Logger.Log(Logger.Error, $"Ошибка запроса деталей: {response.StatusCode}");
            return (null, null, false);
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var threadResponse = JsonSerializer.Deserialize<ThreadResponse>(jsonResponse);
        var threadDetails = threadResponse?.thread;

        if (threadDetails == null || threadDetails.first_post == null || 
            string.IsNullOrEmpty(threadDetails.thread_title) || 
            string.IsNullOrEmpty(threadDetails.first_post.post_body_plain_text))
        {
            Logger.Log(Logger.Error, "Ошибка: получены некорректные данные о теме.");
            return (null, null, false);
        }

        string content = threadDetails.first_post.post_body_plain_text;

        // Проверяем, содержит ли текст `[MEDIA]` или ссылки на вложения
        bool hasMedia = content.Contains("[MEDIA]") || 
                        content.Contains("https://cdn.") || 
                        threadDetails.first_post.post_body_html.Contains("<video") || 
                        threadDetails.first_post.post_body_html.Contains("<img");

        return (threadDetails.thread_title, content, hasMedia);
    }


}

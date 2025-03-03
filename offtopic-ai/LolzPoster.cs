using System.Text.Json;
using offtopic_ai.utils;

namespace offtopic_ai;

class LolzPoster
{
    private static readonly HttpClient client = new HttpClient();
    private const string apiUrl = "https://api.zelenka.guru/posts";
    private const string apiKey = "";

    public async Task<bool> PostReply(int threadId, string message)
    {
        var requestBody = new
        {
            thread_id = threadId,
            post_body = message
        };

        var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
        {
            Content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Authorization", $"Bearer {apiKey}");
        Thread.Sleep(3000);
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            Logger.Log(Logger.Error, $"Ошибка отправки поста: {response.StatusCode}");
            return false;
        }

        return true;
    }
}

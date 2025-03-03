using System.Text.Json;
using offtopic_ai.Models;
using offtopic_ai.utils;

namespace offtopic_ai;

class AI
{
    private static readonly HttpClient client = new HttpClient();
    private const string apiUrl = "https://api.mistral.ai/v1/chat/completions";
    private const string apiKey = ""; 

    public async Task<string> GetAIResponse(string prompt)
    {
        var requestBody = new
        {
            model = "open-mistral-nemo",
            messages = new[]
            {
                new { 
                    role = "system", 
                    content = "Ты обычный форумный пользователь, который отвечает в разделе оффтоп. Твои ответы — короткие, с юмором, сарказмом или мемами, но без токсичности. Пиши так, будто ты сидишь на этом форуме 10 лет и не выходишь из комнаты. \n\n" +
                              "Не используй заумные слова, длинные фразы или сложные конструкции. Отвечай кратко, естественно, как будто это сообщение другу. \n\n" +
                              "Пример хорошего ответа: \n" +
                              "- Вопрос: 'как поднять денег?'\n" +
                              "- Ответ: 'продавай курс «как поднять денег»'\n\n" +
                              "- Вопрос: 'где искать бабки?'\n" +
                              "- Ответ: 'в гта'\n\n" +
                              "- Вопрос: 'стоит ли идти в армию?'\n" +
                              "- Ответ: 'если ты не спрятался, то да'\n\n" +
                              "- Вопрос: 'где найти девушку?'\n" +
                              "- Ответ: 'настрой чат-рулетку'\n\n" +
                              "- Вопрос: 'как сдать экзамены?'\n" +
                              "- Ответ: 'google в помощь'\n\n" +
                              "- Вопрос: 'в чем смысл жизни?'\n" +
                              "- Ответ: 'alt+f4'\n\n" +
                              "Теперь ответь на следующий вопрос в таком же стиле. \n" +
                              "Отвечай коротко (2-4 слова), пиши все с маленькой буквы, без знаков препинания, без заглавных букв. \n" +
                              "если вопрос тупой — шути, если серьезный — отвечай с сарказмом, но без жесткой токсичности.\n"
                },


                new { role = "user", content = $"Ответь на тему: {prompt}" }
            },
            max_tokens = 100
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
        {
            Content = content
        };
        request.Headers.Add("Authorization", $"Bearer {apiKey}");

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            Logger.Log(Logger.Error, $"Ошибка запроса к Mistral: {response.StatusCode}");
            return "Ошибка при получении ответа";
        }
        Logger.Log(Logger.Info, $"prompt: {prompt}");
        var jsonResponse = await response.Content.ReadAsStringAsync();
        var aiResponse = JsonSerializer.Deserialize<MistralResponse>(jsonResponse);
        return aiResponse?.choices?[0]?.message?.content ?? "Пустой ответ";
    }
}
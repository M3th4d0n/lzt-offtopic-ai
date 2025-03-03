namespace offtopic_ai.Models;

class MistralResponse
{
    public Choice[] choices { get; set; }
}
class Choice
{
    public Message message { get; set; }
}
class Message
{
    public string content { get; set; }
}
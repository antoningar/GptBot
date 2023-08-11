using System.Text.Json.Serialization;
using GptBot.UseCase.SubmitPrompt;

namespace GptBot.Gpt;

public record GptAskBody
{
    [JsonPropertyName("model")]
    public string Model { get; set; }
    [JsonPropertyName("messages")]
    public List<GptMessage> Messages { get; set; }

    public GptAskBody(string model, List<Message> messages)
    {
        Model = model;
        Messages = messages.Select(x => new GptMessage(x)).ToList();
    }
}

public record GptMessage
{
    [JsonPropertyName("role")]
    public string? Role { get; set; }
    
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    public GptMessage(Message message)
    {
        Role = message.Role;
        Content = message.Content;
    }
}
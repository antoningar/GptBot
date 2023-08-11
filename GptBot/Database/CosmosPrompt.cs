using GptBot.UseCase.SubmitPrompt;

namespace GptBot.Database;

public record CosmosPrompt
{
    public string? id { get; set; }
    public List<Message>? Conversation { get; init; }
}


using GptBot.UseCase.SubmitPrompt;

namespace GptBot.UseCase;

public interface IPrompts
{
    Task SaveConversation(string username, List<Message> conversation);
    Task<List<Message>?> GetConversation(string username);
    Task ClearConversation(string username);
}
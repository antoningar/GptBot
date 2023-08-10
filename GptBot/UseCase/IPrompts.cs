namespace GptBot.UseCase;

public interface IPrompts
{
    Task SavePrompt(string username, string prompt);
    Task<string> GetConversation(string username);
}
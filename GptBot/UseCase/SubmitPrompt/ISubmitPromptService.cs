namespace GptBot.UseCase.SubmitPrompt;

public interface ISubmitPromptService
{
     Task<List<Message>> SubmitPrompt(string username, string prompt);
     void SavePrompt(string username, List<Message> messages);
}
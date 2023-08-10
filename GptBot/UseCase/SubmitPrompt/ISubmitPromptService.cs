namespace GptBot.UseCase.SubmitPrompt;

public interface ISubmitPromptService
{
     Task<string> SubmitPromt(string username, string prompt);
}
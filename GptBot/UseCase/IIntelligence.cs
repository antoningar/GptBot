namespace GptBot.UseCase;

public interface IIntelligence
{
    public Task<string> Ask(string prompt);
}
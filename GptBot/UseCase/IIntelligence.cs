using GptBot.UseCase.SubmitPrompt;

namespace GptBot.UseCase;

public interface IIntelligence
{
    public Task<Message?> Ask(List<Message> conversation);
}
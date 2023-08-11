namespace GptBot.UseCase.ClearHistory;

public class ClearHistoryService : IClearHistoryService
{
    private readonly IPrompts _prompts;

    public ClearHistoryService(IPrompts prompts)
    {
        _prompts = prompts;
    }

    public async Task Clearhistory(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("username can't be null or empty");

        await _prompts.ClearConversation(username);
    }
}
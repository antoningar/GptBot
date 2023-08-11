using Discord.Interactions;
using GptBot.UseCase.ClearHistory;
using GptBot.UseCase.SubmitPrompt;

namespace GptBot.Discord;

public class DiscordSlashModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ISubmitPromptService _promptService;
    private readonly IClearHistoryService _clearHistoryService;

    public DiscordSlashModule(ISubmitPromptService promptService, IClearHistoryService clearHistoryService)
    {
        _promptService = promptService;
        _clearHistoryService = clearHistoryService;
    }

    [SlashCommand("gpt-submit-prompt", "Submit your promt")]
    public async Task Submit(string prompt)
    {
        await RespondAsync("Hi, I'm working on it.");
     
        string username = Context.User.Username;
        List<Message> messages = await _promptService.SubmitPrompt(username ,prompt);
        string response = "Sorry, error getting the response";
        if (messages.Count >= 1)
            response = messages.Last()!.Content!;
        
        await ReplyAsync(response);

        _promptService.SavePrompt(username, messages);
    }

    [SlashCommand("gpt-clear-history", "Clear your session")]
    public async Task Clear()
    {
        string username = Context.User.Username;
        await _clearHistoryService.Clearhistory(username);
        await RespondAsync("Cleared");
    }
}
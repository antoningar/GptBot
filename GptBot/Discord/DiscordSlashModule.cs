using Discord.Interactions;

namespace GptBot.Discord;

public class DiscordSlashModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("gpt-submit-prompt", "Submit your promt")]
    public async Task Submit(string prompt)
    {
        await RespondAsync(prompt);
    }

    [SlashCommand("gpt-clear-history", "Clear your session")]
    public async Task Clear()
    {
        await RespondAsync("Cleared");
    }
}
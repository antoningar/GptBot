namespace GptBot.UseCase.SubmitPrompt;

public class SubmitPromptService : ISubmitPromptService
{
    private readonly IPrompts _prompts;
    private readonly IIntelligence _intelligence;

    public SubmitPromptService(IPrompts prompts, IIntelligence intelligence)
    {
        _prompts = prompts;
        _intelligence = intelligence;
    }

    public async Task<string> SubmitPromt(string username, string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt) || string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("at least one parameter empty");
        
        await _prompts.SavePrompt(username, prompt);
        string conversation = await _prompts.GetConversation(username);

        string result = await _intelligence.Ask(conversation);
        
        return result;
    }
}
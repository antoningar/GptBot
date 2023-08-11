namespace GptBot.UseCase.SubmitPrompt;

public class SubmitPromptService : ISubmitPromptService
{
    private const string USER_ROLE = "user";
    
    private readonly IPrompts _prompts;
    private readonly IIntelligence _intelligence;

    public SubmitPromptService(IPrompts prompts, IIntelligence intelligence)
    {
        _prompts = prompts;
        _intelligence = intelligence;
    }

    public async Task<List<Message>> SubmitPrompt(string username, string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt) || string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("at least one parameter empty");
        
        List<Message>? conversation = await _prompts.GetConversation(username) ?? new List<Message>();
        conversation.Add(new Message() { Role = USER_ROLE, Content = prompt });

        Message? message = await _intelligence.Ask(conversation);
        if (message == null)
            return new List<Message>();

        conversation.Add(message);
        return conversation;
    }

    public void SavePrompt(string username, List<Message> messages)
    {
        _prompts.SaveConversation(username, messages);
    }
}
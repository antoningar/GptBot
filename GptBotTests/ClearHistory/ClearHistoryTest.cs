using GptBot.UseCase;
using GptBot.UseCase.ClearHistory;
using NSubstitute;
using Xunit;

namespace GptBotTests.ClearHistory;

public class ClearHistoryTest
{
    private static ClearHistoryService GetClearHistoryService
    (
        IPrompts? prompts = null
    )
    {
        IPrompts promptsSubistitute = prompts ?? Substitute.For<IPrompts>();
        return new ClearHistoryService(promptsSubistitute);
    }
        
    [Fact]
    public async Task ClearHistory_ShouldCheckInput()
    {
        ClearHistoryService service = GetClearHistoryService();
        await Assert.ThrowsAsync<ArgumentException>(() => service.Clearhistory(""));
    }
    
    [Fact]
    public async Task ClearHistory_ShouldCallPromtClear()
    {
        const string USERNAME = "bahtiens";
        IPrompts promptsService = Substitute.For<IPrompts>();
        await promptsService
            .ClearConversation(USERNAME);
        
        ClearHistoryService service = GetClearHistoryService(prompts: promptsService);
        await service.Clearhistory(USERNAME);
        
        await promptsService.Received().ClearConversation(USERNAME);
    }
}
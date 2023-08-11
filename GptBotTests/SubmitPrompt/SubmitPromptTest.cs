using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GptBot.UseCase;
using GptBot.UseCase.SubmitPrompt;
using NSubstitute;
using Xunit;

namespace GptBot.tests.SubmitPrompt;

public class SubmitPromptTest
{
    private static SubmitPromptService GetSubmitPromptService(
        IPrompts? prompts = null,
        IIntelligence? intelligence = null)
    {
        IPrompts promptsSubistitute = prompts ?? Substitute.For<IPrompts>();
        IIntelligence intelligenceSubstitute = intelligence ?? Substitute.For<IIntelligence>();
        return new SubmitPromptService(promptsSubistitute, intelligenceSubstitute);
    }
    
    [Theory]
    [InlineData("", "Quel est la capital de la France ?")]
    [InlineData("cestmoi", "")]
    private async Task SubmitPrompt_ShouldCheckInput(string username, string prompt)
    {
        SubmitPromptService service = GetSubmitPromptService();
        await Assert.ThrowsAsync<ArgumentException>(() => service.SubmitPrompt(username, prompt));
    }
    
    [Fact]
    private async Task SubmitPrompt_ShouldReturnResponse()
    {
        IIntelligence intelligence = Substitute.For<IIntelligence>();
        intelligence
            .Ask(Arg.Any<List<Message>>())
            .Returns(new Message { Content = "Je ne sais pas" });
        SubmitPromptService service = GetSubmitPromptService(intelligence: intelligence);
        List<Message> response = await service.SubmitPrompt("username", "okletsgo");
        Assert.True(response.Count > 0);
    }
}
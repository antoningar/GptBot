namespace GptBot.UseCase.ClearHistory;

public interface IClearHistoryService
{
    Task Clearhistory(string username);
}
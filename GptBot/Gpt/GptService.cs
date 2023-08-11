using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using GptBot.UseCase;
using GptBot.UseCase.SubmitPrompt;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace GptBot.Gpt;

public class GptService : IIntelligence
{
    private const string APIKEY_KEY = "GPT:ApiKey";
    private const string MODEL_KEY = "GPT:Model";
    
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public GptService(ILogger logger, IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;

        if (configuration == null)
            throw new ApplicationException("empty configuration");

        _model = configuration.GetSection(MODEL_KEY).Value!;
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {configuration.GetSection(APIKEY_KEY).Value}");
    }

    public async Task<Message?> Ask(List<Message> conversation)
    {
        const string URL = "https://api.openai.com/v1/chat/completions";
        
        GptAskBody body = new(_model, conversation);
        string jsonBody = JsonSerializer.Serialize(body);
        HttpRequestMessage request = new(HttpMethod.Post, URL);
        request.Content = new StringContent(jsonBody);
        request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
        
        _logger.Information("{Class}.{Method}: Querying ChatGPT", nameof(GptService), nameof(Ask));
        HttpResponseMessage responseMessage = await _httpClient.SendAsync(request);
        _logger.Information("{Class}.{Method}: Response {StatusCode} from ChatGPT", nameof(GptService), nameof(Ask), responseMessage.StatusCode);

        string responseStr = await responseMessage.Content.ReadAsStringAsync();
        JsonNode nodeResponse = JsonSerializer.Deserialize<JsonNode>(responseStr)!;

        if (responseMessage.StatusCode != HttpStatusCode.OK)
            return null;
        
        //logs token used
        int promptTokens = nodeResponse["usage"]!["prompt_tokens"]!.GetValue<int>();
        int completionTokens = nodeResponse["usage"]!["completion_tokens"]!.GetValue<int>();
        _logger.Information("{Class}.{Method}: {PromptTokens} prompt tokens, {CompletionTokens} completion tokens",
            nameof(GptService), nameof(Ask), promptTokens, completionTokens);

        string response = nodeResponse["choices"]![0]!["message"]!["content"]!.GetValue<string>();
        return new Message()
        {
            Role = "assistant",
            Content = response
        };
    }
}
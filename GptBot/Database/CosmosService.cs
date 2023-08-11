using GptBot.UseCase;
using GptBot.UseCase.SubmitPrompt;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace GptBot.Database;

public class CosmosService : IPrompts
{
    private const string COSMOSENDPOINT_KEY = "COSMOS:Endpoint";
    private const string COSMOSKEY_KEY = "COSMOS:Key";
    private const string COSMOSCONTAINER_KEY = "COSMOS:Container";
    private const string COSMOSDATABASE_KEY = "COSMOS:Database";

    private readonly ILogger _logger;
    
    private readonly IConfiguration _configuration;
    
    public CosmosService(IConfiguration configuration, ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SaveConversation(string username, List<Message> conversation)
    {
        using CosmosClient client = new(
            accountEndpoint: _configuration.GetSection(COSMOSENDPOINT_KEY).Value,
            authKeyOrResourceToken: _configuration.GetSection(COSMOSKEY_KEY).Value
        );
        
        Container container = GetContainer(client);

        CosmosPrompt prompt = new()
        {
            id = username,
            Conversation = conversation
        };
        
        try
        {
            _logger.Information("{Class}.{Method}: Saving new messages in Cosmos", nameof(CosmosService), nameof(SaveConversation));
            ItemResponse<CosmosPrompt> item = await container.ReplaceItemAsync(
                id: username,
                partitionKey: new PartitionKey(username),
                item: prompt
            );
            Console.WriteLine(item.StatusCode);
        }
        catch (CosmosException ex)
        {
            _logger.Information("{Class}.{Method}: Bad Cosmos response {Response}", nameof(CosmosService), nameof(SaveConversation), ex.ResponseBody);
        }
        _logger.Information("{Class}.{Method}: messages saved in Cosmos", nameof(CosmosService), nameof(SaveConversation));
    }

    public async Task<List<Message>?> GetConversation(string username)
    {
        using CosmosClient client = new(
            accountEndpoint: _configuration.GetSection(COSMOSENDPOINT_KEY).Value,
            authKeyOrResourceToken: _configuration.GetSection(COSMOSKEY_KEY).Value
        );

        Container container = GetContainer(client);

        List<Message>? conversation = new();
        _logger.Information("{Class}.{Method}: Querying cosmos DB", nameof(CosmosService), nameof(GetConversation));
        
        try
        {
            CosmosPrompt prompt = await container.ReadItemAsync<CosmosPrompt>(
                id: username,
                partitionKey: new PartitionKey(username)
            );
            conversation = prompt.Conversation;
        }
        catch (CosmosException ex)
        {
            _logger.Information("{Class}.{Method}: Bad Cosmos response {Response}", nameof(CosmosService), nameof(GetConversation), ex.ResponseBody);
        }

        _logger.Information("{Class}.{Method}: Queried cosmos DB", nameof(CosmosService), nameof(GetConversation));
        return conversation;
    }

    private Container GetContainer(CosmosClient client)
    {
        return client.GetContainer(
            _configuration.GetSection(COSMOSDATABASE_KEY).Value,
            _configuration.GetSection(COSMOSCONTAINER_KEY).Value
        );
    }
}
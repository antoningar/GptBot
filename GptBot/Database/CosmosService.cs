using System.Net;
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
            await container.ReplaceItemAsync(
                id: username,
                partitionKey: new PartitionKey(username),
                item: prompt
            );
        }
        catch (CosmosException ex)
        {
            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.Information("{Class}.{Method}: Old conversation not found, creating an other one", nameof(CosmosService), nameof(SaveConversation));
                await container.CreateItemAsync(
                    partitionKey: new PartitionKey(username),
                    item: prompt
                );
            }
            else
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
            if (ex.StatusCode == HttpStatusCode.NotFound)
                _logger.Information("{Class}.{Method}: New conversation, history not found", nameof(CosmosService), nameof(GetConversation));
            else
                _logger.Information("{Class}.{Method}: Bad Cosmos response {Response}", nameof(CosmosService), nameof(GetConversation), ex.ResponseBody);
        }

        _logger.Information("{Class}.{Method}: Queried cosmos DB", nameof(CosmosService), nameof(GetConversation));
        return conversation;
    }

    public async Task ClearConversation(string username)
    {
        using CosmosClient client = new(
            accountEndpoint: _configuration.GetSection(COSMOSENDPOINT_KEY).Value,
            authKeyOrResourceToken: _configuration.GetSection(COSMOSKEY_KEY).Value
        );

        Container container = GetContainer(client);
        
        _logger.Information("{Class}.{Method}: Querying cosmos DB for delete", nameof(CosmosService), nameof(ClearConversation));
        try
        {
            await container.DeleteItemAsync<CosmosPrompt>(id: username, partitionKey: new PartitionKey(username));
        }
        catch (CosmosException ex)
        {
            _logger.Information("{Class}.{Method}: Bad Cosmos response {Response}", nameof(CosmosService), nameof(ClearConversation), ex.ResponseBody);
        }
        _logger.Information("{Class}.{Method}: Cosmos DB deleted queries ended", nameof(CosmosService), nameof(ClearConversation));
    }

    private Container GetContainer(CosmosClient client)
    {
        return client.GetContainer(
            _configuration.GetSection(COSMOSDATABASE_KEY).Value,
            _configuration.GetSection(COSMOSCONTAINER_KEY).Value
        );
    }
}
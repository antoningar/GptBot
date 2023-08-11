using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace GptBot.Discord;

public class DiscordSlashHandler
{
    private const string GUILD_UUID_KEY = "DISCORD:GUILD_UUID";

    private readonly DiscordSocketClient _client;
    private readonly InteractionService _handler;
    private readonly IServiceProvider _service;
    private readonly IConfiguration _configuration;

    private readonly ILogger _logger;

    public DiscordSlashHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider service, IConfiguration configuration, ILogger logger)
    {
        _client = client;
        _handler = handler;
        _service = service;
        _configuration = configuration;

        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _client.Ready += ReadyAsync;
        _handler.Log += LogAsync;

        await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _service);

        _client.InteractionCreated += HandleInteraction;
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        SocketInteractionContext context = new(_client, interaction);
        await _handler.ExecuteCommandAsync(context, _service);
    }

    private async Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
    }

    private async Task ReadyAsync()
    {
        _logger.Information("{Class}.{Method}: Bot ready", nameof(DiscordSlashHandler), nameof(ReadyAsync));
        if (_configuration.GetSection("MODE").Value == "Debug")
        {
            _logger.Debug("{Class}.{Method}: Program is debug", nameof(DiscordSlashHandler), nameof(ReadyAsync));
            await _handler.RegisterCommandsToGuildAsync(
                Convert.ToUInt64(_configuration.GetSection(GUILD_UUID_KEY).Value));
        }
        else
        {
            await _handler.RegisterCommandsGloballyAsync();
            _logger.Debug("{Class}.{Method}: Program is not debug", nameof(DiscordSlashHandler), nameof(ReadyAsync));            
        }
    }
}
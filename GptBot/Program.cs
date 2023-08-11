using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GptBot.Database;
using GptBot.Discord;
using GptBot.Gpt;
using GptBot.UseCase;
using GptBot.UseCase.SubmitPrompt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;

namespace GptBot
{
    internal static class Program
    {
        private const string TOKEN_KEY = "DISCORD:TOKEN";

        private static readonly DiscordSocketConfig SocketConfig = new()
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
            MaxWaitBetweenGuildAvailablesBeforeReady = 10000
        };
        private static void Main(string[] args)
            => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            
            Logger logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            await using ServiceProvider services = ConfigureServices(configuration, logger);

            DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
            client.Log += LogAsync;
            
            await services.GetRequiredService<DiscordSlashHandler>().InitializeAsync();

            await client.LoginAsync(TokenType.Bot, configuration.GetRequiredSection(TOKEN_KEY).Value);
            await client.StartAsync();
            
            logger.Information("Program started");
            await Task.Delay(Timeout.Infinite);
        }
        private static ServiceProvider ConfigureServices(IConfiguration configuration, ILogger log)
        {
            return new ServiceCollection()
                .AddSingleton(SocketConfig)
                .AddSingleton(configuration)
                .AddSingleton(log)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<DiscordSlashHandler>()
                .AddSingleton<ISubmitPromptService, SubmitPromptService>()
                .AddSingleton<IPrompts, CosmosService>()
                .AddSingleton<IIntelligence, GptService>()
                .AddSingleton<HttpClient>()
                .BuildServiceProvider();
        }

        private static Task LogAsync(LogMessage log)
        {
            Log.Information("{Log}", log.ToString());
            return Task.CompletedTask;
        }

        public static bool IsDebug()
        {
            #if DEBUG
                return true;
            #else
                return false;
            #endif
        }
    }
}
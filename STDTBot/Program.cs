using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using STDTBot.Services;
using System;
using System.Threading.Tasks;

namespace STDTBot
{
    public class Program
    {
        private DiscordSocketClient _client;
        private IConfigurationRoot _config = ConfigService.GetConfiguration();

        static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            var services = new ServiceCollection()
            .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig { LogLevel = Discord.LogSeverity.Debug }))
            .AddSingleton(new CommandService(new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Async,
                LogLevel = Discord.LogSeverity.Debug
            }))
            .AddSingleton<StartupService>()
            .AddSingleton<LoggingService>()
            .AddSingleton(_config);

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<LoggingService>();
            await provider.GetRequiredService<StartupService>().StartAsync();

            await Task.Delay(-1);
        }
    }
}

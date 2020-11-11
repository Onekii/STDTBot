using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using STDTBot.Database;
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
            .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig { LogLevel = Discord.LogSeverity.Info }))
            .AddSingleton(new CommandService(new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Async,
                LogLevel = Discord.LogSeverity.Info
            }))
            .AddSingleton<StartupService>()
            .AddSingleton<CommandHandler>()
            .AddSingleton<LoggingService>()
            .AddSingleton(_config)
            .AddDbContext<STDTContext>(options => options.UseMySQL(BuildConnectionString()));

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<LoggingService>();
            await provider.GetRequiredService<StartupService>().StartAsync();
            provider.GetRequiredService<CommandHandler>();

            await Task.Delay(-1);
        }

        private string BuildConnectionString()
        {
            return new MySqlConnectionStringBuilder()
            {
                Server = _config["database:server"],
                Password = _config["database:password"],
                Database = _config["database:db"],
                UserID = _config["database:user"],
                Port = uint.Parse(_config["database:port"])
            }
.ConnectionString;
        }
    }
}

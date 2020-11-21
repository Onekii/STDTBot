using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using NLog;
using STDTBot.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STDTBot.Services
{
    class CooldownService
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly DiscordSocketClient _client;
        private readonly IConfigurationRoot _config;
        private readonly STDTContext _db;
        private readonly Timer _timer;


        public CooldownService(DiscordSocketClient client, IConfigurationRoot config, STDTContext db)
        {
            _client = client;
            _config = config;
            _db = db;

            _timer = new Timer(_ =>
             {
                 var tasks = new List<Task>();
                 tasks.Add(Task.Factory.StartNew(async () => { await CheckCooldowns().ConfigureAwait(false); }));
             },
             null,
             TimeSpan.Zero,
             TimeSpan.FromSeconds(3));
        }

        private async Task CheckCooldowns()
        {
            int amount = Globals.Cooldowns.RemoveAll(x => x.Expires < DateTime.UtcNow);

            _log.Debug($"Removed {amount} cooldowns for General Messages + Stream Tips");
        }
    }
}

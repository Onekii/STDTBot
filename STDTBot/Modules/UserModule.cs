using Discord.Commands;
using NLog;
using STDTBot.Database;
using STDTBot.Models;
using STDTBot.Utils;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace STDTBot.Modules
{
    public class UserModule : ModuleBase<SocketCommandContext>
    {
        private readonly STDTContext _db;
        private readonly Logger _log = LogManager.GetCurrentClassLogger();


        public UserModule(STDTContext db) : base()
        {
            _db = db;
        }

        [Command("lookup")]
        public async Task LookupUser(ulong userId)
        {
            User u = _db.Users.Find(userId);

            await Context.Channel.SendMessageAsync("", false, Embeds.UserLookup(u)).ConfigureAwait(false);
        }
    }
}

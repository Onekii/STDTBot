using Discord;
using Discord.Commands;
using NLog;
using STDTBot.Database;
using STDTBot.Models;
using STDTBot.Services;
using STDTBot.Utils.Preconditions;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace STDTBot.Modules
{
    public class StaffModule : ModuleBase<SocketCommandContext>
    {
        private readonly STDTContext _db;
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly CommandHandler _commands;

        public StaffModule(STDTContext db, CommandHandler commandHandler)
        {
            _db = db;
            _commands = commandHandler;
        }

        [PermissionCheck]
        [Command("insertusers")]
        public async Task InsertAllUsers()
        {
            List<User> usersToInsert = new List<User>();

            foreach (IGuildUser guildUser in Context.Guild.Users)
            {
                if (_db.Users.Find((long)guildUser.Id) != null)
                    continue;

                User u = new User
                {
                    ID = (long)guildUser.Id,
                    Username = guildUser.Username,
                    Discriminator = guildUser.Discriminator,
                    Left = new DateTime(1900, 01, 01),
                    UserAvatar = guildUser.GetAvatarUrl(),
                    IsStreaming = false,
                    HistoricPoints = 0,
                    CurrentRank = 0,
                    CurrentPoints = 0,
                    CurrentNickname = guildUser.Nickname
                };

                if (guildUser.JoinedAt.HasValue)
                    u.Joined = guildUser.JoinedAt.Value.LocalDateTime;
                else
                    u.Joined = DateTime.Today;

                usersToInsert.Add(u);
            }
            _db.Users.AddRange(usersToInsert);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }


        [PermissionCheck]
        [Command("startraid")]
        public async Task StartRaid()
        {
            RaidInfo newRaid = new RaidInfo();
            newRaid.DateOfRaid = DateTime.UtcNow;

            _db.Raids.Add(newRaid);
            await _db.SaveChangesAsync();

            Globals._activeRaid = newRaid;
        }
        
        [PermissionCheck]
        [Command("stopraid")]
        public async Task StopRaid()
        {
            await _commands.RaidFinished().ConfigureAwait(false);
            Globals._activeRaid = null;
        }
    }
}

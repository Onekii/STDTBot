using Discord;
using Discord.Commands;
using NLog;
using STDTBot.Database;
using STDTBot.Models;
using STDTBot.Services;
using STDTBot.Utils.Preconditions;
using System;
using System.Collections.Generic;
using System.Linq;
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
        [Command("CheckCurrentRoles")]
        public async Task CheckRoles()
        {
            List<long> CurrentRoleIDs = _db.Ranks.ToList().Select(x => x.OfflineRole).ToList();
            foreach (IGuildUser U in Context.Guild.Users)
            {
                foreach (var role in U.RoleIds)
                {
                    IRole a = Context.Guild.GetRole(role);
                    _log.Info($"User {U.Username} has roles: {a.Name}");

                    if (CurrentRoleIDs.Contains((long)role))
                    {
                        RankInfo r = _db.Ranks.First(x => x.OfflineRole == (long)role);
                        //u.CurrentRank = r.ID;
                        //u.CurrentPoints = r.PointsNeeded;
                        //u.HistoricPoints = r.PointsNeeded;
                        _log.Info($"Found User {U.Username} has rank: {r.Name} and Role ID: {role}");
                    }
                }
            }
        }

        [PermissionCheck]
        [Command("insertusers")]
        public async Task InsertAllUsers()
        {
            List<User> usersToInsert = new List<User>();
            List<long> CurrentRoleIDs = _db.Ranks.ToList().Select(x => x.OfflineRole).ToList();

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

                foreach (var role in guildUser.RoleIds)
                {
                    if (CurrentRoleIDs.Contains((long)role))
                    {
                        RankInfo r = _db.Ranks.First(x => x.OfflineRole == (long)role);
                        if (r.Name == "Rookie") continue;
                        u.CurrentRank = r.ID;
                        u.CurrentPoints = r.PointsNeeded;
                        u.HistoricPoints = r.PointsNeeded;
                        _log.Info($"Assigned Role {r.Name} to {u.Username}");
                    }
                }

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

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Configuration;
using MySql.Data.EntityFrameworkCore.Query.Internal;
using NLog;
using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Crypto.Engines;
using STDTBot.Database;
using STDTBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace STDTBot.Services
{
    public class CommandHandler
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _provider;
        private IGuild guild;
        private readonly STDTContext _db;
        private ulong _referralChannelId;

        private Dictionary<IUser, DateTime> _voiceMembersJoinedTimer { get; set; }

        internal enum CooldownType
        {
            StreamTips,
            GeneralMessage
        }
        internal struct Cooldown
        {
            internal CooldownType Type { get; set; }
            internal ulong UserId { get; set; }
            internal DateTime Expires { get; set; }
        }


        public CommandHandler(DiscordSocketClient discord, CommandService commands, IConfigurationRoot config, IServiceProvider services, STDTContext context)
        {
            _client = discord;
            _commands = commands;
            _config = config;
            _provider = services;
            _db = context;

            _client.UserJoined += OnUserJoined;
            _client.MessageReceived += OnMessageReceieved;
            _client.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
            _client.GuildMemberUpdated += OnUserUpdated;
            _client.GuildAvailable += OnGuildAvailable;



            _voiceMembersJoinedTimer = new Dictionary<IUser, DateTime>();
        }

        private async Task OnGuildAvailable(SocketGuild arg)
        {
            guild = arg;
            var channel = await GetReferralsChannel().ConfigureAwait(false);
            _referralChannelId = channel.Id;
        }

        private async Task OnUserUpdated(SocketGuildUser prev, SocketGuildUser now)
        {
            User u = null;

            if (prev.Activity is null && now.Activity != null)
            {
                if (now.Activity.Type == ActivityType.Streaming)
                {
                    u = _db.Users.Find((long)now.Id);
                    u.IsStreaming = true;

                    await AssignStreamingRole(now, true);
                }
            }
            else if (prev.Activity != null && now.Activity != null)
            {
                if (prev.Activity.Type != ActivityType.Streaming && now.Activity.Type == ActivityType.Streaming)
                {
                    u = _db.Users.Find((long)now.Id);
                    u.IsStreaming = true;

                    await AssignStreamingRole(now, true);
                }
            }
            if (prev.Activity != null && prev.Activity.Type == ActivityType.Streaming)
            {
                if (now.Activity is null || (now.Activity != null && now.Activity.Type != ActivityType.Streaming))
                {
                    u = _db.Users.Find((long)now.Id);
                    u.IsStreaming = false;

                    await AssignStreamingRole(now, false);
                }
            }

            if (u != null)
            {
                _db.Entry(u).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await _db.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private async Task OnUserJoined(SocketGuildUser user)
        {
            User existingUser = _db.Users.Find((long)user.Id);
            if (existingUser is null)
            {
                existingUser = new User()
                {
                    ID = (long)user.Id,
                    Username = user.Username,
                    Joined = DateTime.UtcNow,
                    UserAvatar = user.GetAvatarUrl(),
                    CurrentNickname = user.Nickname,
                    Discriminator = user.Discriminator,
                    CurrentPoints = 0,
                    HistoricPoints = 0,
                    CurrentRank = 0,
                    IsStreaming = false,
                    Left = new DateTime(1900, 1, 1)
                };
                _db.Users.Add(existingUser);
                await _db.SaveChangesAsync().ConfigureAwait(false);
            }
            else
            {
                await AssignRankRole(user);
            }
        }

        private async Task AssignRankRole(IGuildUser guildUser, User user = null)
        {
            if (user is null)
                user = _db.Users.Find((long)guildUser.Id);

            RankInfo rank = _db.Ranks.FirstOrDefault(x => x.ID == user.CurrentRank);
            if (rank is null)
                return;

            if (user.IsStreaming)
                await guildUser.AddRoleAsync(guildUser.Guild.GetRole((ulong)rank.OnlineRole)).ConfigureAwait(false);
            else
                await guildUser.AddRoleAsync(guildUser.Guild.GetRole((ulong)rank.OfflineRole)).ConfigureAwait(false);
        }

        private async Task RemoveRankRole(IGuildUser guildUser, User user = null)
        {
            if (user is null)
                user = _db.Users.Find((long)guildUser.Id);

            RankInfo rank = _db.Ranks.FirstOrDefault(x => x.ID == user.CurrentRank);
            if (rank is null)
                return;

            if (user.IsStreaming)
                await guildUser.RemoveRoleAsync(guildUser.Guild.GetRole((ulong)rank.OnlineRole)).ConfigureAwait(false);
            else
                await guildUser.RemoveRoleAsync(guildUser.Guild.GetRole((ulong)rank.OfflineRole)).ConfigureAwait(false);
        }

        internal async Task AssignStreamingRole(IGuildUser user, bool online)
        {
            User u = _db.Users.Find((long)user.Id);
            RankInfo ri = _db.Ranks.Find(u.CurrentRank);

            if (ri is null)
            {
                _log.Error($"RankInfo is null!, Current rank is {u.CurrentRank}, User is {u.Username}");
                return; 
            }

            long roleToRemove = (online ? ri.OfflineRole : ri.OnlineRole);
            long roleToAdd = (online ? ri.OnlineRole : ri.OfflineRole);

            await user.RemoveRoleAsync(user.Guild.GetRole((ulong)roleToRemove)).ConfigureAwait(false);
            await user.AddRoleAsync(user.Guild.GetRole((ulong)roleToAdd)).ConfigureAwait(false);
        }

        private async Task<IVoiceChannel> GetRaidVoiceChannel()
        {
            return await GetVoiceChannel("RaidVoice");
        }

        internal async Task CheckUsersInRaidChannel()
        {
            IVoiceChannel raidVoiceChannel = await GetRaidVoiceChannel();
            IVoiceChannel muteVoiceChannel = await GetRaidMuteVoiceChannel();

            var usersInRaidVoice = await raidVoiceChannel.GetUsersAsync().FirstAsync();
            var usersInMuteRaid = await muteVoiceChannel.GetUsersAsync().FirstAsync();

            foreach (var user in usersInRaidVoice)
            {
                _voiceMembersJoinedTimer.Add(user, DateTime.UtcNow);

                RaidAttendee dbUser = _db.RaidAttendees.Find((long)user.Id, Globals._activeRaid.RaidID);
                if (dbUser is null)
                {
                    dbUser = new RaidAttendee()
                    {
                        RaidID = Globals._activeRaid.RaidID,
                        UserID = (long)user.Id,
                        MinutesInRaid = 0,
                        PointsObtained = 0
                    };
                }

                _db.RaidAttendees.Add(dbUser);
            }

            foreach (var user in usersInMuteRaid)
            {
                _voiceMembersJoinedTimer.Add(user, DateTime.UtcNow);

                RaidAttendee dbUser = _db.RaidAttendees.Find((long)user.Id, Globals._activeRaid.RaidID);
                if (dbUser is null)
                {
                    dbUser = new RaidAttendee()
                    {
                        RaidID = Globals._activeRaid.RaidID,
                        UserID = (long)user.Id,
                        MinutesInRaid = 0,
                        PointsObtained = 0
                    };
                }

                _db.RaidAttendees.Add(dbUser);
            }

            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {

            if (Globals._activeRaid is null)
                return;

            IVoiceChannel raidVoiceChannel = await GetRaidVoiceChannel();
            IVoiceChannel muteVoiceChannel = await GetRaidMuteVoiceChannel();

            // See if you can find a better way to do this.
            if (oldState.VoiceChannel == raidVoiceChannel && newState.VoiceChannel == muteVoiceChannel
    || oldState.VoiceChannel == muteVoiceChannel && newState.VoiceChannel == raidVoiceChannel)
            { // Nothing they're just going to be muted 
                _log.Info($"User {user.Username} moved from {oldState.VoiceChannel.Name} to {newState.VoiceChannel.Name}. No action required.");
                return;
            }


            if (oldState.VoiceChannel != raidVoiceChannel && newState.VoiceChannel == raidVoiceChannel
                || oldState.VoiceChannel != muteVoiceChannel && newState.VoiceChannel == muteVoiceChannel)
            //Joined Raid Channel
            {
                _voiceMembersJoinedTimer.Add(user, DateTime.UtcNow);

                RaidAttendee dbUser = _db.RaidAttendees.Find((long)user.Id, Globals._activeRaid.RaidID);
                if (dbUser is null)
                {
                    dbUser = new RaidAttendee()
                    {
                        RaidID = Globals._activeRaid.RaidID,
                        UserID = (long)user.Id,
                        MinutesInRaid = 0,
                        PointsObtained = 0
                    };
                    _db.RaidAttendees.Add(dbUser);
                }

                _log.Info($"User {user.Username} joined a raid chat. Adding to table.");
            }

            else if (oldState.VoiceChannel == raidVoiceChannel && newState.VoiceChannel != raidVoiceChannel
                || oldState.VoiceChannel == muteVoiceChannel && newState.VoiceChannel != muteVoiceChannel)
            //Left Raid Channel
            {
                _log.Info($"User {user.Username} left a raid chat. Calculating points.");
                await UserLeftRaid(user).ConfigureAwait(false);
            }


            // No Action Required, Not using raid channel
            else { }

            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task<IVoiceChannel> GetRaidMuteVoiceChannel()
        {
            return await GetVoiceChannel("RaidVoiceMute");
        }

        private async Task UserLeftRaid(IUser user)
        {
            DateTime leftTime = DateTime.UtcNow;
            DateTime joinedTime = _voiceMembersJoinedTimer[user];
            _voiceMembersJoinedTimer.Remove(user);

            int pointsPerTen = int.Parse(_db.Config.First(x => x.Name == "RaidPoints").Value);

            double minutesInRaid = leftTime.Subtract(joinedTime).TotalMinutes;
            minutesInRaid -= (minutesInRaid % 10);

            double pointAmount = Math.Round(minutesInRaid / 10) * pointsPerTen;

            RaidAttendee dbUser = await _db.RaidAttendees.FindAsync((long)user.Id, Globals._activeRaid.RaidID);
            dbUser.MinutesInRaid += (int)minutesInRaid;
            dbUser.PointsObtained += (int)pointAmount;

            _db.Entry(dbUser).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            await LogPointsForAction(_db.Users.Find(dbUser.UserID), PointReasons.Raid, (int)pointAmount);
        }

        private async Task OnMessageReceieved(SocketMessage message)
        {
            var msg = message as SocketUserMessage;

            if (msg == null)
                return;

            if (msg.Author == _client.CurrentUser)
                return;

            var context = new SocketCommandContext(_client, msg);

            if (await HandleSpecialChannels(msg, context).ConfigureAwait(false))
                return;

            int argPos = 0;
            if (msg.HasStringPrefix(_config["prefix"], ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                _log.Info($"{msg.Author.Username} (in {msg?.Channel?.Name}/{context?.Guild?.Name}) is trying to execute: " + msg.Content);
                var result = await _commands.ExecuteAsync(context, argPos, _provider);

                if (!result.IsSuccess)
                {
                    _log.Info(result.ToString());
                    await msg.DeleteAsync().ConfigureAwait(false);
                }
            }
            else
            {
                if (Globals.Cooldowns.Any(x => x.Type == CooldownType.GeneralMessage && x.UserId == msg.Author.Id))
                    return;

                User dbUser = _db.Users.Find((long)msg.Author.Id);
                int points = int.Parse(_db.Config.First(x => x.Name == "GeneralMessagePoints").Value);
                await AddPointsToUser(dbUser, (long)points).ConfigureAwait(false);
                await LogPointsForAction(dbUser, PointReasons.GeneralMessage, points).ConfigureAwait(false);

                int interval = int.Parse(_db.Config.First(x => x.Name == "GeneralMessageInterval").Value);

                Globals.Cooldowns.Add(new Cooldown()
                {
                    UserId = dbUser.GetID(),
                    Type = CooldownType.GeneralMessage,
                    Expires = DateTime.UtcNow.AddSeconds(interval)
                });

            }
        }

        private async Task<bool> HandleSpecialChannels(SocketUserMessage msg, SocketCommandContext context)
        {
            var specialChannel = _db.SpecialChannels.Find((long)msg.Channel.Id);

            if (specialChannel is null)
                return false;

            int argPos = 0;
            if (msg.HasStringPrefix(_config["prefix"], ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
                return false;

            switch (specialChannel.ChannelType.ToLower())
            {
                case "referrals":
                    {
                        await HandleReferral(msg).ConfigureAwait(false);
                        return true;
                    }
                case "streamtips":
                    {
                        if (Globals.Cooldowns.Any(x => x.Type == CooldownType.StreamTips && x.UserId == msg.Author.Id))
                            return true;

                        User dbUser = _db.Users.Find((long)msg.Author.Id);
                        int points = int.Parse(_db.Config.First(x => x.Name == "StreamTipsPoints").Value);

                        await AddPointsToUser(dbUser, (long)points).ConfigureAwait(false);
                        await LogPointsForAction(dbUser, PointReasons.StreamTips, points).ConfigureAwait(false);

                        int interval = int.Parse(_db.Config.First(x => x.Name == "StreamTipsInterval").Value);
                        Globals.Cooldowns.Add(new Cooldown()
                        {
                            UserId = dbUser.GetID(),
                            Type = CooldownType.GeneralMessage,
                            Expires = DateTime.UtcNow.AddSeconds(interval)
                        });

                        return true;
                    }
            }

            return false;
        }

        private async Task HandleReferral(SocketUserMessage msg)
        {
            long referralAmount = int.Parse(_db.Config.First(x => x.Name == "ReferralPoints").Value);

            IUser user = msg.MentionedUsers.First();

            Referral r = new Referral()
            {
                ID = (long)msg.Author.Id,
                ReferredBy = (long)user.Id,
                ReferralTime = DateTime.UtcNow
            };

            _db.Referrals.Add(r);
            await _db.SaveChangesAsync().ConfigureAwait(false);

            User dbUser = _db.Users.Find((long)user.Id);
            await AddPointsToUser(dbUser, referralAmount).ConfigureAwait(false);
            await LogPointsForAction(dbUser, PointReasons.Referral, (int)referralAmount);
        }

        private async Task DecayServerPoints()
        {
            long decayAmount = 0;

            foreach (User u in _db.Users)
            {
                if (u.CurrentPoints > decayAmount)
                    u.CurrentPoints -= decayAmount;
                else
                    u.CurrentPoints = 0;

                _db.Entry(u).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await OnUserPointsChanged(u).ConfigureAwait(false);
            }

            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task AddPointsToUser(User u, long amount)
        {
            u.CurrentPoints += amount;
            u.HistoricPoints += amount;
            _db.Entry(u).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await OnUserPointsChanged(u).ConfigureAwait(false);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task OnUserPointsChanged(User u)
        {
            List<RankInfo> ranks = _db.Ranks.ToList().OrderBy(x => x.PointsNeeded).ToList();
            RankInfo currentRank = (_db.Ranks.FirstOrDefault(x => x.ID == u.CurrentRank));
            int rankIndex = ranks.IndexOf(currentRank);
            RankInfo nextRank = null;
            RankInfo previousRank = null;

            if (rankIndex < (ranks.Count - 1) && u.CurrentPoints >= ranks[rankIndex + 1].PointsNeeded)
                nextRank = ranks[rankIndex + 1];

            if (rankIndex > 0 && u.CurrentPoints < ranks[rankIndex - 1].PointsNeeded)
                previousRank = ranks[rankIndex - 1];

            IGuildUser guildUser = await guild.GetUserAsync((ulong)u.ID);

            if (nextRank != null)
            {
                if (u.CurrentPoints >= nextRank.PointsNeeded)
                {
                    await RemoveRankRole(guildUser, u);
                    u.CurrentRank = nextRank.ID;
                    await AssignRankRole(guildUser, u);
                    //Send announcement
                }
            }
            if (previousRank != null)
            {
                if (u.CurrentPoints < previousRank.PointsNeeded)
                {
                    await RemoveRankRole(guildUser, u);
                    u.CurrentRank = previousRank.ID;
                    await AssignRankRole(guildUser, u);
                }
            }
        }

        internal async Task RaidFinished()
        {
            foreach (var pair in _voiceMembersJoinedTimer)
            {
                await UserLeftRaid(pair.Key).ConfigureAwait(false);
            }

            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task<IGuildChannel> GetReferralsChannel()
        {
            return await GetSpecialChannel("Referrals");
        }

        private async Task<IGuildChannel> GetSpecialChannel(string channelType)
        {
            SpecialChannel channel = _db.SpecialChannels.First(x => x.ChannelType == channelType);

            return await guild.GetChannelAsync(channel.GetChannelID()).ConfigureAwait(false);
        }

        private async Task<IVoiceChannel> GetVoiceChannel(string channelType)
        {
            SpecialChannel channel = _db.SpecialChannels.First(x => x.ChannelType == channelType);

            return await guild.GetVoiceChannelAsync(channel.GetChannelID()).ConfigureAwait(false);
        }


        internal enum PointReasons
        {
            GeneralMessage,
            StreamTips,
            Raid,
            Referral
        }

        private async Task LogPointsForAction(User dbUser, PointReasons reason, int points)
        {
            _db.MIData.Add(new
                MIData()
            {
                UserId = dbUser.ID,
                PointsLogged = points,
                ReasonLogged = (int)reason,
                DateLogged = DateTime.UtcNow
            }
            );

            await _db.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}

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
        private readonly STDTContext _db;
        private ulong _referralChannelId;
        uint pointsPerThird = 0;

        private Dictionary<IUser, DateTime> _voiceMembersJoinedTimer { get; set; }

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

            _voiceMembersJoinedTimer = new Dictionary<IUser, DateTime>();
        }

        private async Task OnUserUpdated(SocketGuildUser prev, SocketGuildUser now)
        {
            // hardcoded, change to config somewhere
            //STDT Test
            //IGuild guild = _client.GetGuild(761587244699484160);
            //STDT live
            //IGuild guild = _client.GetGuild(751253055378423838);

            User u = null;

            if (prev.Activity is null && now.Activity != null)
            {
                if (now.Activity.Type == ActivityType.Streaming)
                {
                    u = _db.Users.Find(now.Id);
                    u.IsStreaming = true;

                    await AssignStreamingRole(now, true);
                }
            }
            if (prev.Activity != null && prev.Activity.Type == ActivityType.Streaming)
            {
                if (now.Activity is null || (now.Activity != null && now.Activity.Type != ActivityType.Streaming))
                {
                    u = _db.Users.Find(now.Id);
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
            User existingUser = _db.Users.Find(user.Id);
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

            RankInfo rank = _db.Ranks.First(x => x.ID == user.CurrentRank);

            if (user.IsStreaming)
                await guildUser.AddRoleAsync(guildUser.Guild.GetRole((ulong)rank.OnlineRole)).ConfigureAwait(false);
            else
                await guildUser.AddRoleAsync(guildUser.Guild.GetRole((ulong)rank.OfflineRole)).ConfigureAwait(false);
        }

        private async Task RemoveRankRole(IGuildUser guildUser, User user = null)
        {
            if (user is null)
                user = _db.Users.Find((long)guildUser.Id);

            RankInfo rank = _db.Ranks.First(x => x.ID == user.CurrentRank);

            if (user.IsStreaming)
                await guildUser.RemoveRoleAsync(guildUser.Guild.GetRole((ulong)rank.OnlineRole)).ConfigureAwait(false);
            else
                await guildUser.RemoveRoleAsync(guildUser.Guild.GetRole((ulong)rank.OfflineRole)).ConfigureAwait(false);
        }

        private async Task AssignStreamingRole(IGuildUser user, bool online)
        {
            User u = _db.Users.Find(user.Id);
            RankInfo ri = _db.Ranks.Find(u.CurrentRank);

            long roleToRemove = (online ? ri.OfflineRole : ri.OnlineRole);
            long roleToAdd = (online ? ri.OnlineRole : ri.OfflineRole);

            await user.RemoveRoleAsync(user.Guild.GetRole((ulong)roleToRemove)).ConfigureAwait(false);
            await user.RemoveRoleAsync(user.Guild.GetRole((ulong)roleToAdd)).ConfigureAwait(false);
        }

        private async Task<IVoiceChannel> GetRaidVoiceChannel()
        {
            return await GetVoiceChannel("RaidVoice");
        }

        private async Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {

            if (Globals._activeRaid is null)
                return;

            IVoiceChannel raidVoiceChannel = await GetRaidVoiceChannel();

            // See if you can find a better way to do this.
            if (oldState.VoiceChannel != raidVoiceChannel && newState.VoiceChannel == raidVoiceChannel)
            //Joined Raid Channel
            {
                _voiceMembersJoinedTimer.Add(user, DateTime.Now);

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
            else if (oldState.VoiceChannel == raidVoiceChannel && newState.VoiceChannel != raidVoiceChannel)
            //Left Raid Channel
            {
                await UserLeftRaid(user).ConfigureAwait(false);
            }
            // No Action Required, Not using raid channel
            else { }

            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task UserLeftRaid(IUser user)
        {
            DateTime leftTime = DateTime.Now;
            DateTime joinedTime = _voiceMembersJoinedTimer[user];
            _voiceMembersJoinedTimer.Remove(user);

            double minutesInRaid = leftTime.Subtract(joinedTime).TotalMinutes;
            double pointAmount = Math.Round(minutesInRaid / 3) * pointsPerThird;

            RaidAttendee dbUser = _db.RaidAttendees.Find((long)user.Id, Globals._activeRaid.RaidID);
            dbUser.MinutesInRaid += (int)minutesInRaid;
            dbUser.PointsObtained += (int)pointAmount;

            _db.Entry(dbUser).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        }

        private async Task OnMessageReceieved(SocketMessage message)
        {
            var msg = message as SocketUserMessage;
            var context = new SocketCommandContext(_client, msg);

            if (msg == null)
                return;

            if (msg.Author == _client.CurrentUser)
                return;

            if (_referralChannelId == 0)
            {
                var x = await GetReferralsChannel().ConfigureAwait(false);
                _referralChannelId = x.Id;
            }

            if (msg.Channel.Id == _referralChannelId)
            {
                if (msg.MentionedUsers.Count != 1)
                {
                    await msg.DeleteAsync();
                    return;
                }

                await HandleReferral(msg).ConfigureAwait(false);
            }

            int argPos = 0;
            if (msg.HasStringPrefix(_config["prefix"], ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                _log.Info($"{msg.Author.Username} (in {msg?.Channel?.Name}/{context?.Guild?.Name}) is trying to execute: " + msg.Content);
                var result = await _commands.ExecuteAsync(context, argPos, _provider);

                if (!result.IsSuccess)
                {
                    _log.Error(result.ToString());
                    await msg.DeleteAsync().ConfigureAwait(false);
                }
            }
        }

        private async Task HandleReferral(SocketUserMessage msg)
        {
            long referralAmount = 1;

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

            // Get Rookie Role
            User u = _db.Users.Find((long)msg.Author.Id);

            // hardcoded, change to config somewhere
            //STDT Test
            IGuild guild = _client.GetGuild(761587244699484160);
            //STDT live
            //IGuild guild = _client.GetGuild(751253055378423838);
            IGuildUser guildUser = await guild.GetUserAsync(msg.Author.Id).ConfigureAwait(false);

            u.CurrentRank = _db.Ranks.First(x => x.PointsNeeded == 0).ID;

            await AssignRankRole(guildUser, u).ConfigureAwait(false);
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
            _db.Entry(u).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await OnUserPointsChanged(u).ConfigureAwait(false);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task OnUserPointsChanged(User u)
        {
            // hardcoded, change to config somewhere
            //STDT Test
            IGuild guild = _client.GetGuild(761587244699484160);
            //STDT live
            //IGuild guild = _client.GetGuild(751253055378423838);

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
            // hardcoded, change to config somewhere
            //STDT Test
            IGuild guild = _client.GetGuild(761587244699484160);
            //STDT live
            //IGuild guild = _client.GetGuild(751253055378423838);

            SpecialChannel channel = _db.SpecialChannels.First(x => x.ChannelType == channelType);

            return await guild.GetChannelAsync(channel.GetChannelID()).ConfigureAwait(false);
        }

        private async Task<IVoiceChannel> GetVoiceChannel(string channelType)
        {
            // hardcoded, change to config somewhere
            //STDT Test
            IGuild guild = _client.GetGuild(761587244699484160);
            //STDT live
            //IGuild guild = _client.GetGuild(751253055378423838);

            SpecialChannel channel = _db.SpecialChannels.First(x => x.ChannelType == channelType);

            return await guild.GetVoiceChannelAsync(channel.GetChannelID()).ConfigureAwait(false);
        }
    }
}

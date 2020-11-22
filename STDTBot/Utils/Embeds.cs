using Discord;
using Microsoft.Extensions.Configuration;
using STDTBot.Database;
using STDTBot.Models;
using STDTBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STDTBot.Utils
{
    class Embeds
    {
        private static IConfigurationRoot _config = ConfigService.GetConfiguration();

        internal static Embed UserLookup(User u)
        {
            throw new NotImplementedException();
        }

        internal static Embed RaidEnded(STDTContext db, RaidInfo ri, IGuild guild)
        {
            List<RaidAttendee> Attendees = db.RaidAttendees.ToList().Where(x => x.RaidID == Globals._activeRaid.RaidID).ToList();
            var emb = new EmbedBuilder()
            {
                Color = Globals.SuccessColor,
                Description = $"Raid {ri.RaidID} (started {ri.DateOfRaid}) Successfully Ended!\r\n" +
                $"Counted {Attendees.Count} attendees.\r\n" + GetAttendeesString(),
                Footer = new EmbedFooterBuilder().WithIconUrl(_config[$"guilds:{guild.Id}:logo"]).WithText(_config[$"guilds:{guild.Id}:name"])
            };

            string GetAttendeesString() 
            {
                StringBuilder sb = new StringBuilder();
                Attendees.ForEach(x =>
                {
                    User DBuser = db.Users.Find(x.UserID);

                    sb.AppendLine($"{DBuser.Username} - {x.MinutesInRaid} minutes - {x.PointsObtained} points.");
                });

                return sb.ToString();
            }

            return emb.Build();
        }

        internal static Embed RaidStarted(RaidInfo ri, IGuildUser user)
        {
            var emb = new EmbedBuilder()
            {
                Color = Globals.SuccessColor,
                Description = $"Raid {ri.RaidID} started by {user.Username}!",
                Footer = new EmbedFooterBuilder().WithIconUrl(_config[$"guilds:{user.GuildId}:logo"]).WithText(_config[$"guilds:{user.GuildId}:name"])
            };

            return emb.Build();
        }
    }
}

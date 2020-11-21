using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using STDTBot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STDTBot.Utils.Preconditions
{
    class PermissionCheck : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var user = context.User as IGuildUser;
            var channel = context.Channel as IGuildChannel;
            var guild = context.Guild as IGuild;
            var config = services.GetService(typeof(IConfigurationRoot)) as IConfigurationRoot;
            var db = services.GetService(typeof(STDTContext)) as STDTContext;


            if (config == null)
                return PreconditionResult.FromError("Unable to Process Permission Check.. Config is null!");

            ulong bot = ulong.Parse(config["tokens:botId"]);
            ulong owner = ulong.Parse(config["tokens:ownerId"]);

            if (context.User.Id == bot || context.User.Id == owner)
                return PreconditionResult.FromSuccess();

            bool channelPerms = CommandAllowedInChannel(command, db, channel);
            if (channelPerms)
                if (CommandAllowedByRole(command, db, user, guild))
                    return PreconditionResult.FromSuccess();
                else
                    return PreconditionResult.FromError($"User {user.Id} - {user.Username}#{user.Discriminator} tried to execute command {command.Name} that is not allowed by their current role!");
            else
                return PreconditionResult.FromError($"User {user.Id} - {user.Username}#{user.Discriminator} tried to execute command {command.Name} that is not allowed in channel {channel.Name}");
        }

        private bool CommandAllowedInChannel(CommandInfo command, STDTContext db, IGuildChannel channel)
        {
            List<long> AllowedChannelIDs = db.CommandChannelPermissions.ToList().Where(x => x.CommandName.ToLower() == command.Name.ToLower()).Select(x => x.ChannelID).ToList();
            if (AllowedChannelIDs.Count == 0)
                return true;

            return AllowedChannelIDs.Contains((long)channel.Id);
        }

        private bool CommandAllowedByRole(CommandInfo command, STDTContext db, IGuildUser user, IGuild guild)
        {
            var perm = db.CommandRolePermissions.Find(command.Name);
            if (perm is null)
                return true;

            var guildRoles = guild.Roles;
            var userMinRole = guild.GetRole((ulong)perm.MinimumRole);

            foreach (var role in guildRoles)
            {

                if (role.Position >= userMinRole.Position)
                    return true;
            }

            return false;
        }
    }
}

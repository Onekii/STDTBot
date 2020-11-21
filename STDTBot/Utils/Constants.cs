using System;
using System.Collections.Generic;
using System.Text;
using static STDTBot.Services.CommandHandler;

namespace STDTBot
{
    internal static class Globals
    {
        internal const string ConfigFileName = "config.json";

        internal static STDTBot.Models.RaidInfo _activeRaid;
        internal static List<Cooldown> Cooldowns = new List<Cooldown>();
    }
}

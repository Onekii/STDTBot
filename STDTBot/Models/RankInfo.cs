using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace STDTBot.Models
{
    class RankInfo
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public long OfflineRole { get; set; }
        public long OnlineRole { get; set; }
        public long PointsNeeded { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace STDTBot.Models
{
    class SpecialChannel
    {
        public long ChannelID { get; set; }
        public string ChannelType { get; set; }

        public ulong GetChannelID() => (ulong)ChannelID;
    }
}

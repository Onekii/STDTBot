using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace STDTBot.Models
{
    public class User
    {
        public long ID { get; set; }
        public string Username { get; set; }
        public string Discriminator { get; set; }
        public string CurrentNickname { get; set; }
        public DateTime Joined { get; set; }
        public DateTime Left { get; set; }
        public string UserAvatar { get; set; }
        public long HistoricPoints { get; set; }
        public long CurrentPoints { get; set; }
        public long CurrentRank { get; set; }
        public bool IsStreaming { get; set; }

        public ulong GetID() => (ulong)ID;
    }
}

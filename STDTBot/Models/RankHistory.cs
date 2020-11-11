using System;
using System.Collections.Generic;
using System.Text;

namespace STDTBot.Models
{
    class RankHistory
    {
        public long ID { get; set; }
        public long UserID { get; set; }
        public DateTime DateReset { get; set; }
        public long RankID { get; set; }
    }
}

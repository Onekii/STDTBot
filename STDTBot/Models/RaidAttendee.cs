using System;
using System.Collections.Generic;
using System.Text;

namespace STDTBot.Models
{
    class RaidAttendee
    {
        public long UserID { get; set; }
        public long RaidID { get; set; }
        public long MinutesInRaid { get; set; }
        public long PointsObtained { get; set; }
    }
}

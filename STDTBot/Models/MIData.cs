using System;
using System.Collections.Generic;
using System.Text;

namespace STDTBot.Models
{
    class MIData
    {
        public long ID { get; set; }
        public long UserId { get; set; }
        public int PointsLogged { get; set; }
        public int ReasonLogged { get; set; }
        public DateTime DateLogged { get; set; }
    }
}

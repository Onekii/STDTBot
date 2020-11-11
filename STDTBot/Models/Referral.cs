using System;
using System.Collections.Generic;
using System.Text;

namespace STDTBot.Models
{
    class Referral
    {
        public long ID { get; set; }
        public long ReferredBy { get; set; }
        public DateTime ReferralTime { get; set; }
    }
}

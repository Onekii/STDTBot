using System;
using System.Collections.Generic;
using System.Text;

namespace STDTBot.Models
{
    class CommandRolePermission
    {
        public string CommandName { get; set; }
        public long MinimumRole { get; set; }
    }
}

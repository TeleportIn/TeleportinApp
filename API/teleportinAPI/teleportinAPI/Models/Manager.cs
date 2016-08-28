using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace teleportinAPI.Models
{
    public class Manager
    {
        public string userID { get; set; }
        public List<User> users { get; set; }
        public string openTokSession { get; set; } 
    }
}
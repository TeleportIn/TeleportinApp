using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace TeleportInServer.Models
{
    public class SessionResponse
    {

        public SessionResponse(string sess, string tok)
        {
            this.apikey = ConfigurationManager.AppSettings["App_Key"]; ;
            this.sessionId = sess;
            this.token = tok;
        }

        public SessionResponse()
        {
            this.apikey = ConfigurationManager.AppSettings["App_Key"]; ;
        }

        public string apikey { get; set; }
        public string sessionId { get; set; }
        public string token { get; set; }
    }


}
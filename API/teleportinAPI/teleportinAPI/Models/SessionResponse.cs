using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace teleportinAPI.Models
{
    public class SessionResponse
    {
        public string apiKey { get; set; }
        public string sessionId { get; set; }
        public string token { get; set; }

        public SessionResponse(string sess, string tok)
        {
            this.apiKey = ConfigurationManager.AppSettings["API_KEY"];
            this.sessionId = sess;
            this.token = tok;
        }

        public SessionResponse()
        {
            apiKey = ConfigurationManager.AppSettings["API_KEY"];
        }
    }
}
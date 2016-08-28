using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;

namespace teleportinAPI.Models
{
    public class TeleportSession: TableEntity
    {
        public string SessionId { get; set; }

        public int Channel { get; set; }

        public string ApiKey { get; set; }

        public TeleportSession(string sessionId, int channel)
        {
            PartitionKey = ConfigurationManager.AppSettings["API_KEY"];
            this.SessionId = sessionId;
            RowKey = channel.ToString();
        }

        public TeleportSession()
        {
            PartitionKey = ConfigurationManager.AppSettings["API_KEY"];
        }
    }
}
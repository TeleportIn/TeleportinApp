using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace TeleportInServer.Models
{
    public class TeleportSession : TableEntity
    {
        public TeleportSession(string SessionId, int channel)
        {
            this.PartitionKey = ConfigurationManager.AppSettings["App_Key"];
            this.SessionId = SessionId;
            this.RowKey = channel.ToString();
        }

        public TeleportSession() { }

        public string SessionId { get; set; }
        public int channel { get; set; }
        public string ApiKey { get; set; }
    }
}
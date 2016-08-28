using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using OpenTokSDK;
using System.Configuration;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using TeleportInServer.Models;
using System.Web.Http.Cors;

namespace TeleportInServer.Controllers
{

    [EnableCors(origins: "https://teleportin.azurewebsites.net", headers: "*", methods: "*")]
    public class SessionController : ApiController
    {

        // GET: api/Session
        public IEnumerable<string> Get()
        {
            return new string[] { "Error", "Must Supply a channel" };
        }

        // GET: api/session?channel=5
        public SessionResponse Get(int channel)
        {
            int ApiKey = int.Parse(ConfigurationManager.AppSettings["App_Key"]); // YOUR API KEY
            string ApiSecret = ConfigurationManager.AppSettings["App_Secret"];
            var OpenTok = new OpenTok(ApiKey, ApiSecret);
            var sessionResponse = new SessionResponse();

            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings["StorageConnectionString"]);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "sessions" table.
            CloudTable table = tableClient.GetTableReference("sessions");

            string chan = channel.ToString();
            // Create a retrieve operation that takes a TeleportSession entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<TeleportSession>(ApiKey.ToString(),chan);

            // Execute the retrieve operation.
            TableResult currentSession = table.Execute(retrieveOperation);

            if(currentSession == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            sessionResponse.sessionId = ((TeleportSession)currentSession.Result).SessionId;
            sessionResponse.token = OpenTok.GenerateToken(sessionResponse.sessionId);
            return sessionResponse;
            
        }

        // POST: api/Session
        public void Post([FromBody]SessionChannel chan)
        {
            int ApiKey = int.Parse(ConfigurationManager.AppSettings["App_Key"]); // YOUR API KEY
            string ApiSecret = ConfigurationManager.AppSettings["App_Secret"];
            int channel = chan.channel;

            var OpenTok = new OpenTok(ApiKey, ApiSecret);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.AppSettings["StorageConnectionString"]);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("sessions");

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();

            TableOperation retrieveOperation = TableOperation.Retrieve<TeleportSession>(ApiKey.ToString(), chan.ToString());
            string sessionId;

            // Execute the retrieve operation.
            TableResult currentSession = table.Execute(retrieveOperation);
            if (currentSession.Result != null)
            {
                sessionId = ((TeleportSession)currentSession.Result).SessionId;
            }
            else
            {
                // Create a session that uses the OpenTok Media Router (which is required for archiving)
                var session = OpenTok.CreateSession(mediaMode: MediaMode.ROUTED);
                // Store this sessionId in the database for later use:
                sessionId = session.Id;
            }

            TeleportSession newSesh = new TeleportSession(sessionId, channel);

            TableOperation insertOperation = TableOperation.Insert(newSesh);
            try
            {
                TableResult tableresult = table.Execute(insertOperation);
            }
            catch (Exception itemexists)
            {
                throw new Exception("Session has already been created with that Id and channel");
            }

        }

        // PUT: api/Session/5
        public void Put(int id, [FromBody]string value)
        {
           
        }

        // DELETE: api/Session/5
        public void Delete(int id)
        {
        }
    }
}

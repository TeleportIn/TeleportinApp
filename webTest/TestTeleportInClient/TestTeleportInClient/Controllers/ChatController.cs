using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using NascarWebPOC.Utils;
using Newtonsoft.Json;
using TestTeleportInClient.Models;

namespace TestTeleportInClient.Controllers
{
    public class ChatController : Controller
    {
        private string POCApiResourceid = ConfigurationManager.AppSettings["apiResourceID"];
        private string POCApiBaseAddress = ConfigurationManager.AppSettings["apiBaseAddress"];
        private const string TenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string appKey = ConfigurationManager.AppSettings["ida:ClientSecret"];

        // GET: Chat
        public async Task<ActionResult> Index()
        {
            AuthenticationResult result = null;
            List<OpenTokToken> itemList = new List<OpenTokToken>();

            try
            {
                string userObjectID =
                    ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")
                        .Value;
                AuthenticationContext authContext = new AuthenticationContext(Startup.Authority,
                    new NaiveSessionCache(userObjectID));
                ClientCredential credential = new ClientCredential(clientId, appKey);
                result =
                    await
                        authContext.AcquireTokenSilentAsync(POCApiResourceid, credential,
                            new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));

                //
                // Retrieve the user's Race List.
                //
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, POCApiBaseAddress + "session?channel=1");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                HttpResponseMessage response = await client.SendAsync(request);

                //
                // Return the Race list in the view.
                //
                if (response.IsSuccessStatusCode)
                {
                    List<Dictionary<String, String>> responseElements = new List<Dictionary<String, String>>();
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    String responseString = await response.Content.ReadAsStringAsync();
                    responseElements = JsonConvert.DeserializeObject<List<Dictionary<String, String>>>(responseString,
                        settings);
                    foreach (Dictionary<String, String> responseElement in responseElements)
                    {
                        OpenTokToken newItem = new OpenTokToken();
                        newItem.apiKey = responseElement["apiKey"];
                        newItem.sessionId = responseElement["sessionId"];
                        newItem.token = responseElement["token"];
                        itemList.Add(newItem);
                    }

                    return View(itemList);
                }
                else
                {
                    //
                    // If the call failed with access denied, then drop the current access token from the cache, 
                    //     and show the user an error indicating they might need to sign-in again.
                    //
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        var todoTokens = authContext.TokenCache.ReadItems().Where(a => a.Resource == POCApiResourceid);
                        foreach (TokenCacheItem tci in todoTokens)
                            authContext.TokenCache.DeleteItem(tci);

                        ViewBag.ErrorMessage = "UnexpectedError";
                        OpenTokToken newItem = new OpenTokToken();
                        newItem.apiKey = "(No items in list)";
                        itemList.Add(newItem);
                        return View(itemList);
                    }
                }
            }
            catch (AdalException ee)
            {
                if (Request.QueryString["reauth"] == "True")
                {
                    //
                    // Send an OpenID Connect sign-in request to get a new set of tokens.
                    // If the user still has a valid session with Azure AD, they will not be prompted for their credentials.
                    // The OpenID Connect middleware will return to this controller after the sign-in response has been handled.
                    //
                    HttpContext.GetOwinContext().Authentication.Challenge(
                        new AuthenticationProperties(),
                        OpenIdConnectAuthenticationDefaults.AuthenticationType);
                }

                //
                // The user needs to re-authorize.  Show them a message to that effect.
                //
                OpenTokToken newItem = new OpenTokToken();
                newItem.apiKey = "(Sign-in required to view to do list.)";
                itemList.Add(newItem);
                ViewBag.ErrorMessage = "AuthorizationRequired";
                return View(itemList);
            }


            //
            // If the call failed for any other reason, show the user an error.
            //
            return View("Error");
        }
    }
}
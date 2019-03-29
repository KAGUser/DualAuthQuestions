using Microsoft.Identity.Client;
using System.IdentityModel.Tokens;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System;
using System.Net;
using System.Net.Http;
using System.Globalization;

namespace CO.Outlook.Controllers
{
    [Authorize]
    public class GetTokenController : ApiController
    {
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static string clientIdAddin = ConfigurationManager.AppSettings["ida:ClientIdAddin"];
        private static string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
        private static string enterpriseAppResourceId = ConfigurationManager.AppSettings["todo:enterpriseAppResourceId"];
        private static string enterpriseAppBaseAddress = ConfigurationManager.AppSettings["todo:enterpriseAppBaseAddress"];
        private Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext authContextADAL = null;
        private static string PasswordAddin = ConfigurationManager.AppSettings["ida:PasswordAddin"];
        private HttpClient httpClient = new HttpClient();

        [Authorize]
        // GET api/GetToken
		// This routine creates AzureAd token, another routine creates jwtBearer depending on which works with dual auth with cookies
        public async Task<HttpResponseMessage> Get()
        {
            // OWIN middleware validated the audience and issuer, but the scope must also be validated; must contain "access_as_user".
            string[] addinScopes = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope").Value.Split(' ');
            if (addinScopes.Contains("access_as_user"))
            {
                var bootstrapContext = ClaimsPrincipal.Current.Identities.First().BootstrapContext as BootstrapContext;
                Microsoft.IdentityModel.Clients.ActiveDirectory.UserAssertion userAssertion = new Microsoft.IdentityModel.Clients.ActiveDirectory.UserAssertion(bootstrapContext.Token);

                authContextADAL = new Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext(authority, new FileCache());
                Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult result = null;
                Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential clientCred =
                    new Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential(clientIdAddin, PasswordAddin);
                result = await authContextADAL.AcquireTokenAsync(enterpriseAppResourceId, clientCred, userAssertion);

                HttpResponseMessage response = null;

                try
                {
                    //response = ODataHelper.SendRequestWithAccessToken(enterpriseAppBaseAddress + "api/branches", result.AccessToken);
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.AccessToken);

                    // Call the To Do list service.
                    // Or call to /api/outlook/branches/user
                    response = await httpClient.GetAsync(enterpriseAppBaseAddress + "api/branches/user");

                    if (response.IsSuccessStatusCode)
                    {
                        return response;
                    }
                    else
                    {
                        string failureDescription = await response.Content.ReadAsStringAsync();
                        return SendErrorToClient(HttpStatusCode.Unauthorized, null, $"{response.ReasonPhrase}\n {failureDescription} " + "- An error occurred while getting /api/branches");
                    }
                }
                catch (MsalServiceException e)
                {
                    itemNames.Add("e.Message: " + e.Message);
                }

            }
            // The token from the client does not have "access_as_user" permission.
            return SendErrorToClient(HttpStatusCode.Unauthorized, null, "Missing access_as_user.");
        }

        private HttpResponseMessage SendErrorToClient(HttpStatusCode statusCode, Exception e, string message)
        {
            HttpError error;

            if (e != null)
            {
                error = new HttpError(e, true);
            }
            else
            {
                error = new HttpError(message);
            }
            var requestMessage = new HttpRequestMessage();
            var errorMessage = requestMessage.CreateErrorResponse(statusCode, error);

            return errorMessage;
        }
    }
}

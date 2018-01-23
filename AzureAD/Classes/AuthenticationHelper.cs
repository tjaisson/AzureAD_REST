using System;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;

namespace AzureADAccess.AD
{
    internal class AuthenticationHelper
    {
        public static string tocken;

        public static bool DoReset = false;

        protected static string filePath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "oo365.tk");
            }
        }

        public static bool load()
        {
            if (!System.IO.File.Exists(filePath)) return false;
            tocken = System.IO.File.ReadAllText(filePath);
            return true;
        }

        public static void save()
        {
            System.IO.File.WriteAllText(filePath, tocken ?? "");
        }

        public static async Task<string> GetTocken()
        {
            if (DoReset)
            {
                tocken = null;
            }
            if (tocken == null)
            {
                if ((DoReset) || !load())
                {
                    AuthenticationResult userAuthnResult;
                    Uri redirectUri = new Uri("https://localhost");
                    AuthenticationContext authenticationContext = new AuthenticationContext(Constants.AuthString, false);
                    userAuthnResult = await authenticationContext.AcquireTokenAsync(Constants.ResourceUrl,
                        Constants.ClientIdForUserAuthn, redirectUri, new PlatformParameters(PromptBehavior.Always));
                    tocken = userAuthnResult.AccessToken;
                    save();
                    DoReset = false;
                }
            }
            return tocken;
        }


        /// <summary>
        /// Get Active Directory Client for User.
        /// </summary>
        /// <returns>ActiveDirectoryClient for User.</returns>
        public async static Task<ActiveDirectoryClient> GetActiveDirectoryClientAsUser()
        {
            string accessToken = await GetTocken();

            Uri servicePointUri = new Uri(Constants.ResourceUrl);
            Uri serviceRoot = new Uri(servicePointUri, Constants.TenantId);
            ActiveDirectoryClient activeDirectoryClient = new ActiveDirectoryClient(serviceRoot, GetTocken);
            return activeDirectoryClient;
        }


        public static async Task<Microsoft.Graph.GraphServiceClient> GetGraphServiceClientAsUser()
        {
            string accessToken = await GetTocken();

            Microsoft.Graph.GraphServiceClient GraphServiceClient =
            new Microsoft.Graph.GraphServiceClient(Constants.ResourceUrl + "/" + Constants.TenantId,
            new Microsoft.Graph.DelegateAuthenticationProvider(
            (requestMessage) =>
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);
                UriBuilder ub = new UriBuilder(requestMessage.RequestUri);
                System.Collections.Specialized.NameValueCollection query = System.Web.HttpUtility.ParseQueryString(ub.Query);
                query["api-version"] = "1.5";
                ub.Query = query.ToString();
                requestMessage.RequestUri = ub.Uri;
                return Task.FromResult(0);
            }));

            return GraphServiceClient;
        }


    }
}

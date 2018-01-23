using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace AzureADAccess.AD
{
    internal class BaseGraph
    {
        public static async Task<string> getGroupUid(string gn)
        {
            GraphServiceClient gsc = await AuthenticationHelper.GetGraphServiceClientAsUser();
            IGraphServiceGroupsCollectionPage igcp = await gsc.Groups.Request().Filter("displayName eq '" + gn + "'").GetAsync();
            IList<Group> gl = igcp.CurrentPage;
            if (gl.Count != 1) return null;
            Group g = gl.First();
            return g.AdditionalData["objectId"].ToString();
        }
        public static async Task<string> getUserUid(string upn)
        {
            GraphServiceClient gsc = await AuthenticationHelper.GetGraphServiceClientAsUser();
            IGraphServiceUsersCollectionPage iucp = await gsc.Users.Request().Filter("UserPrincipalName eq '" + upn + "'").GetAsync();
            IList <User> ul = iucp.CurrentPage;
            if (ul.Count != 1) return null;
            User u = ul.First();
            return u.AdditionalData["objectId"].ToString();
        }
    }
}

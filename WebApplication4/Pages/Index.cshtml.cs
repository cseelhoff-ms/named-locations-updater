using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using Microsoft.Graph;
using Azure.Identity;
using Microsoft.Graph.Models;

namespace WebApplication4.Pages
{
    [AuthorizeForScopes(ScopeKeySection = "MicrosoftGraph:Scopes")]
    public class IndexModel : PageModel
    {        
        private readonly GraphServiceClient _graphServiceClient;
        private readonly GraphServiceClient _graphClientApp;
        private readonly ILogger<IndexModel> _logger;
        private readonly ManagedIdentityCredential _credential;
        private string myUserID;


        public IndexModel(ILogger<IndexModel> logger, GraphServiceClient graphServiceClient)
        {
            _logger = logger;
            _graphServiceClient = graphServiceClient;
            string[] graph_scope = new[] { "https://graph.microsoft.com/.default" };
            
            _credential = new ManagedIdentityCredential();
            _graphClientApp = new GraphServiceClient(_credential, graph_scope);
        }

        public async Task<List<string>> GetListOfManagedUsers()
        {
            myUserID = "a";
            if(myUserID == null || myUserID == "")
            {
                myUserID = await GetCurrentUserId();
            }
            //List<User> users = await GetDirectReports();
            // return userprincipalnames of direct reports that are not disabled
            //return users.Where(u => u.AccountEnabled == true).Select(u => u.UserPrincipalName).ToList();
            return new List<string> { myUserID };
        }

        private async Task<string> GetCurrentUserId()
        {
            return null;
        }

        public async Task OnGet()
        {
            List<string> listOfManagedUsers = await GetListOfManagedUsers();
            ViewData["ListOfManagedUsers"] = listOfManagedUsers;
        }

        public async Task OnPost(string selectedUser, string TAPLifetimeInMinutes)
        {
            //convert TAPLifetimeInMinutes to int with a maximum of 240
            int TAPLifetimeInMinutesInt = Math.Min(int.Parse(TAPLifetimeInMinutes), 240);
            ViewData["SelectedUser"] = selectedUser;
            List<string> listOfManagedUsers = await GetListOfManagedUsers();
            ViewData["ListOfManagedUsers"] = listOfManagedUsers;
            if (!listOfManagedUsers.Contains(selectedUser))
            {
                ViewData["TemporaryAccessPass"] = "User is not a direct report";
            }
        }
    }
}
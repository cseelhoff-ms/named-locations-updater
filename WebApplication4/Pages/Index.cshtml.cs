using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using Microsoft.Graph;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Graph.Auth;

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
        private readonly IConfiguration _configuration;                   


        public IndexModel(ILogger<IndexModel> logger, GraphServiceClient graphServiceClient, IConfiguration configuration)
        {
            _logger = logger;
            _graphServiceClient = graphServiceClient;
            _configuration = configuration;
            string clientId = _configuration["ClientId"];
            string clientSecret = _configuration["ClientSecret"];
            string tenantId = _configuration["TenantId"];

            

            var authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";

            var confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(authority)
                .WithClientSecret(clientSecret)
                .Build();

            var authProvider = new ClientCredentialProvider(confidentialClientApplication);
            _graphClientApp = new GraphServiceClient(authProvider);

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


        public async Task<bool> AddEntraUserToGroup(string userPrincipalName, string groupId)
        {
            try
            {
                string userId = (await _graphClientApp.Users[userPrincipalName].Request().GetAsync()).Id;
                await _graphClientApp.Groups[groupId].Members.References.Request().AddAsync(new DirectoryObject { Id = userId });
            } catch (Exception ex)
            {
                //_logger.LogError(ex.Message);
            }
            return true;
        }


        public async Task<string> CreateTemporaryAccessPass(string userId, int TAPLifetimeInMinutesInt)
        {
            TemporaryAccessPassAuthenticationMethod temporaryAccessPassMethod = new()
            {
                IsUsableOnce = true,
                LifetimeInMinutes = TAPLifetimeInMinutesInt
            };
            TemporaryAccessPassAuthenticationMethod createdTemporaryAccessPassMethod =
                await _graphClientApp.Users[userId].Authentication
                .TemporaryAccessPassMethods
                .Request()
                .AddAsync(temporaryAccessPassMethod);
            return createdTemporaryAccessPassMethod.TemporaryAccessPass;
        }

        private async Task<List<User>> GetDirectReports()
        {
            List<User> users = new();
            string userIDcurly = "{" + myUserID + "}";
            IUserDirectReportsCollectionWithReferencesPage directReports = await _graphClientApp.Users[userIDcurly].DirectReports
                .Request()
                .Select("accountEnabled,userPrincipalName")
                .GetAsync();

            if (directReports?.CurrentPage != null)
            {
                foreach (var directReport in directReports.CurrentPage)
                {
                    if (directReport is User user)
                    {
                        if ((bool)user.AccountEnabled)
                        {
                            users.Add(user);
                        }
                    }
                }
            }
            return users;
        }

        private async Task<string> GetCurrentUserId()
        {
            User myUser;
            try {
                myUser = await _graphServiceClient.Me.Request()
                .Select("id")
                .GetAsync();
                return myUser.Id;
            } catch (Exception ex)
            {
                // refresh token by redirecting the user to /.auth/refresh
                Microsoft.AspNetCore.Mvc.RedirectResult redirectResult = Redirect("/.auth/refresh");                
            }
            try {
                myUser = await _graphServiceClient.Me.Request()
                .Select("id")
                .GetAsync();
                return myUser.Id;
            } catch (Exception ex)
            {
                // refresh token by redirecting the user to /.auth/refresh
                Microsoft.AspNetCore.Mvc.RedirectResult redirectResult = Redirect("/.auth/refresh");                
            }
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
            else
            {
                bool groupMembershipResult = await AddEntraUserToGroup(selectedUser, "c8e7927c-86f0-4912-afb1-5f97fd10845d");
                ViewData["TemporaryAccessPass"] = await CreateTemporaryAccessPass(selectedUser, TAPLifetimeInMinutesInt);
            }
        }
    }
}
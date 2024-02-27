using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using Microsoft.Graph;
using Azure.Identity;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using Microsoft.Kiota.Abstractions.Serialization;

namespace WebApplication4.Pages
{
    public class KiotaSerializationConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(SerializationWriterFactoryRegistry).IsAssignableFrom(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var properties = value.GetType().GetProperties()
                .Where(p => p.Name != "ValidContentType");

            writer.WriteStartObject();
            foreach (var property in properties)
            {
                writer.WritePropertyName(property.Name);
                serializer.Serialize(writer, property.GetValue(value));
            }
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
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
            myUserID = "";
            if(myUserID == null || myUserID == "")
            {
                myUserID = await GetCurrentUserId();
            }
            List<string> debuginfo = new List<string> { myUserID };

            //output all properties of the object _graphClientApp... perhaps convert the recursive properties to json

            debuginfo.Add(JsonConvert.SerializeObject(_graphClientApp, Formatting.Indented, new KiotaSerializationConverter()));
            debuginfo.Add(JsonConvert.SerializeObject(_graphServiceClient, Formatting.Indented, new KiotaSerializationConverter()));
            debuginfo.Add(JsonConvert.SerializeObject(_credential, Formatting.Indented, new KiotaSerializationConverter()));

            //List<User> users = await GetDirectReports();
            // return userprincipalnames of direct reports that are not disabled
            //return users.Where(u => u.AccountEnabled == true).Select(u => u.UserPrincipalName).ToList();
            return debuginfo;
        }

        private async Task<string> GetCurrentUserId()
        {
            User myUser;
            try {
                //get id of current user using graphServiceClient
                myUser = await _graphServiceClient.Me.GetAsync();
                return myUser.Id;
            } catch (Exception ex)
            {
                // refresh token by redirecting the user to /.auth/refresh
                Microsoft.AspNetCore.Mvc.RedirectResult redirectResult = Redirect("/.auth/refresh");                
            }
            try {
                myUser = await _graphServiceClient.Me.GetAsync();
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
        }
    }
}
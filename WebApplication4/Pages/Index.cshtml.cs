using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace WebApplication4.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly GraphServiceClient _graphServiceClient;

        public IndexModel(ILogger<IndexModel> logger, GraphServiceClient graphServiceClient)
        {
            _logger = logger;
            List<String> scopes = new List<String>(new[] { "https://graph.microsoft.com/.default" });

            var clientId = "e3a0543b-ade1-4b45-9f1f-a59743bf612d";
            var clientSecret = "1Iq8Q~2IVyKV5sWjrpqvOX_zyTmvPu.Q2uN~Oa1B";
            var tenantId = "00ac9db9-508a-473b-aded-53250025bd24";
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };
            var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret, options);
            _graphServiceClient = new GraphServiceClient(clientSecretCredential, scopes);

            //_graphServiceClient = graphServiceClient;
        }

        public List<NamedLocation> Locations { get; private set; }

        public async Task OnGetAsync()
        {
            NamedLocationCollectionResponse? result = await _graphServiceClient.Identity.ConditionalAccess.NamedLocations.GetAsync((requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Filter = "isof('microsoft.graph.ipNamedLocation')";
            });
            Locations = result?.Value ?? new List<NamedLocation>();
        }
    }
}
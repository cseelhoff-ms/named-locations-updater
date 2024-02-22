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
            _graphServiceClient = new GraphServiceClient(new ManagedIdentityCredential(), scopes);
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
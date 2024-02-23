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
        private readonly ManagedIdentityCredential _managedIdentityCredential;

        public IndexModel(ILogger<IndexModel> logger, GraphServiceClient graphServiceClient)
        {
            _logger = logger;
            List<String> scopes = new List<String>(new[] { "https://graph.microsoft.com/.default" });
            _managedIdentityCredential = new ManagedIdentityCredential();
            _graphServiceClient = new GraphServiceClient(_managedIdentityCredential, scopes);
            Locations = new List<NamedLocation>();
        }

        public List<NamedLocation> Locations { get; private set; }

        public async Task OnGetAsync()
        {
            List<String> scopes = new List<String>(new[] { "https://graph.microsoft.com/.default" });
            GraphServiceClient graphServiceClient = new GraphServiceClient(_managedIdentityCredential, scopes);
            // log print all debug log information for GraphServiceClient graphServiceClient including principal id, scopes, permissions, ect...
            _logger.LogDebug("GraphServiceClient graphServiceClient: {graphServiceClient}", graphServiceClient);
            _logger.LogDebug("GraphServiceClient graphServiceClient: {graphServiceClient}", _graphServiceClient);
            _logger.LogDebug("ManagedIdentityCredential _managedIdentityCredential: {managedIdentityCredential}", _managedIdentityCredential);
            _logger.LogDebug("Scopes: {scopes}", scopes);

            NamedLocationCollectionResponse? result = await graphServiceClient.Identity.ConditionalAccess.NamedLocations.GetAsync();
            Locations = result?.Value ?? new List<NamedLocation>();
        }
    }
}
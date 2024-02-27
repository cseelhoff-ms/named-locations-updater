using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Web;

namespace WebApplication4.Pages
{    
    [AuthorizeForScopes(ScopeKeySection = "MicrosoftGraph:Scopes")]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly GraphServiceClient _graphServiceClient;

        public IndexModel(ILogger<IndexModel> logger, GraphServiceClient graphServiceClient)
        {
            _logger = logger;
            _graphServiceClient = graphServiceClient;;
        }

        public List<NamedLocation> Locations { get; private set; }

        public async Task OnGet()
        {
            NamedLocationCollectionResponse? result = await _graphServiceClient.Identity.ConditionalAccess.NamedLocations.GetAsync((requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Filter = "isof('microsoft.graph.ipNamedLocation')";
            });
            Locations = result?.Value ?? new List<NamedLocation>();
        }
    }
}
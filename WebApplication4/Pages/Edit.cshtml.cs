using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Web;


namespace WebApplication4.Pages
{    
    [AuthorizeForScopes(ScopeKeySection = "MicrosoftGraph:Scopes")]
    public class EditModel : PageModel
    {
        private readonly GraphServiceClient _graphServiceClient;

        [BindProperty]
        public IpNamedLocation Location { get; set; }



        public EditModel(GraphServiceClient graphServiceClient)
        {
            _graphServiceClient = graphServiceClient;
            string[] graph_scope = new[] { "https://graph.microsoft.com/.default" };
            _graphServiceClient = new GraphServiceClient(new ManagedIdentityCredential(), graph_scope);
            Location = new IpNamedLocation();
        }

        public string IpRangeToString(IpRange ipRange)
        {
            switch (ipRange)
            {
                case IPv4CidrRange ipv4Range:
                    return ipv4Range.CidrAddress ?? string.Empty;
                case IPv6CidrRange ipv6Range:
                    return ipv6Range.CidrAddress ?? string.Empty;
                default:
                    throw new ArgumentException("Unknown IP range type", nameof(ipRange));
            }
        }
        
        public async Task<IActionResult> OnGetAsync(string locationId)
        {
            var location = await _graphServiceClient.Identity.ConditionalAccess.NamedLocations[locationId].GetAsync();
            var ipNamedLocation = location as IpNamedLocation;
            if (location == null)
            {
                return NotFound();
            }

            Location = new IpNamedLocation
            {
                Id = ipNamedLocation?.Id,
                DisplayName = ipNamedLocation?.DisplayName,
                IpRanges = ipNamedLocation?.IpRanges,
                IsTrusted = ipNamedLocation?.IsTrusted
            };

            ViewData["IpRanges"] = string.Join("\n", Location.IpRanges?.Select(ipRange => IpRangeToString(ipRange)) ?? Enumerable.Empty<string>());

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Microsoft.Extensions.Primitives.StringValues ipRangesString = Request.Form["IpRanges"];
            List<IPv4CidrRange> ipv4Ranges = ipRangesString.ToString().Split('\n')
                .Select(cidrAddress => new IPv4CidrRange
                {
                    OdataType = "#microsoft.graph.iPv4CidrRange",
                    CidrAddress = cidrAddress.Trim(),
                })
                .ToList();

            List<IpRange> IpRanges = ipv4Ranges.Cast<IpRange>().ToList();

            IpNamedLocation requestBody = new()
            {
                OdataType = "#microsoft.graph.ipNamedLocation",
                //DisplayName = Location.DisplayName,
                //IsTrusted = Location.IsTrusted,
                IpRanges = IpRanges
            };

            var result = await _graphServiceClient.Identity.ConditionalAccess.NamedLocations[Location.Id].PatchAsync(requestBody);

            return RedirectToPage("./Index");
        }
    }
}
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});

List<String> scopes = new List<String>(new[] { "https://graph.microsoft.com/.default" });

/*
builder.Services.AddScoped<GraphServiceClient>(sp =>
{
    return new GraphServiceClient(new ManagedIdentityCredential(), scopes);
});
*/

builder.Services.AddScoped<GraphServiceClient>(sp =>
{
    // Initialize GraphServiceClient with the appropriate authentication provider
    // For example, using client credentials:
    var clientId = "e3a0543b-ade1-4b45-9f1f-a59743bf612d";
    var clientSecret = "1Iq8Q~2IVyKV5sWjrpqvOX_zyTmvPu.Q2uN~Oa1B";
    var tenantId = "00ac9db9-508a-473b-aded-53250025bd24";
    var options = new TokenCredentialOptions
    {
        AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
    };
    var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret, options);
    return new GraphServiceClient(clientSecretCredential, scopes);
});

builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();

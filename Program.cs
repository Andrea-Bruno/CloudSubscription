using CloudSubscription;
using CloudSubscription.Components;
using static CloudSubscription.Settings;
var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;


// load settings from appsettings.json
PayPalBusinessEmail = configuration.GetValue(typeof(string), nameof(PayPalBusinessEmail), null) as string;
ApiEndpoint = configuration.GetValue(typeof(string), nameof(ApiEndpoint) , null) as string;

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Used to get httpContext in razor pages
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

//if (!Debugger.IsAttached)
//{
//    app.Urls.Add("http://*:5444");
//    app.Urls.Add("https://*:80");
//}
// user for PayPal IPN validation
app.UseMiddleware<PayPal.PayPalIpnMiddleware>(Events.OnPaymentCompleted);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();

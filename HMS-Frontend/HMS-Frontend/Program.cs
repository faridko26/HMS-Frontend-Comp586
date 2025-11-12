using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Bind Backend config
builder.Services.Configure<BackendOptions>(
    builder.Configuration.GetSection("Backend"));

builder.Services.AddHttpContextAccessor();   // <-- needed for session token access
builder.Services.AddSession();

// ===== Auth client (you already had this) =====
var useFake = builder.Configuration.GetValue<bool>("Frontend:UseFakeApi");
if (useFake)
{
    builder.Services.AddSingleton<IAuthApiClient, FakeAuthApiClient>();
}
else
{
    builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>((sp, http) =>
    {
        var opts = sp.GetRequiredService<IOptions<BackendOptions>>().Value;
        if (string.IsNullOrWhiteSpace(opts.BaseUrl))
            throw new InvalidOperationException("Backend:BaseUrl is not configured.");
        http.BaseAddress = new Uri(opts.BaseUrl!.TrimEnd('/') + "/");
    });
}

// ===== Users client (ADD THIS) =====
// Handler that attaches "Authorization: Bearer <token>" from session
builder.Services.AddTransient<JwtSessionHandler>();

builder.Services.AddHttpClient<IUsersApiClient, UsersApiClient>((sp, http) =>
{
    var opts = sp.GetRequiredService<IOptions<BackendOptions>>().Value;
    if (string.IsNullOrWhiteSpace(opts.BaseUrl))
        throw new InvalidOperationException("Backend:BaseUrl is not configured.");
    http.BaseAddress = new Uri(opts.BaseUrl!.TrimEnd('/') + "/");
})
.AddHttpMessageHandler<JwtSessionHandler>();

// Pages
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapRazorPages();
app.MapGet("/", context =>
{
    context.Response.Redirect("/Account/SignIn");
    return Task.CompletedTask;
});

app.Run();

public record BackendOptions { public string? BaseUrl { get; init; } }

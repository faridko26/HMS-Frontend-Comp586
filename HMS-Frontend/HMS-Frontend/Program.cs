using Microsoft.Extensions.Options;
using HMS_Frontend.Services;

var builder = WebApplication.CreateBuilder(args);

// Bind Backend config
builder.Services.Configure<BackendOptions>(
    builder.Configuration.GetSection("Backend"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ===== Auth client (you already had this) =====
// ===== Factory Registration =====
builder.Services.AddSingleton<HMS_Frontend.Services.Factories.IApiClientFactory, HMS_Frontend.Services.Factories.ApiClientFactory>();

// ===== Auth client =====
// Register both implementations, Factory will decide which one to return if used manually, 
// or we can use the factory to register the interface.
builder.Services.AddSingleton<FakeAuthApiClient>();
builder.Services.AddHttpClient<AuthApiClient>((sp, http) =>
{
    var opts = sp.GetRequiredService<IOptions<BackendOptions>>().Value;
    if (string.IsNullOrWhiteSpace(opts.BaseUrl))
        throw new InvalidOperationException("Backend:BaseUrl is not configured.");
    http.BaseAddress = new Uri(opts.BaseUrl!.TrimEnd('/') + "/");
});

// Register IAuthApiClient using the Factory
builder.Services.AddTransient<IAuthApiClient>(sp =>
    sp.GetRequiredService<HMS_Frontend.Services.Factories.IApiClientFactory>().CreateAuthClient());


// ===== Users client =====
builder.Services.AddTransient<JwtSessionHandler>();

builder.Services.AddHttpClient<UsersApiClient>((sp, http) =>
{
    var opts = sp.GetRequiredService<IOptions<BackendOptions>>().Value;
    if (string.IsNullOrWhiteSpace(opts.BaseUrl))
        throw new InvalidOperationException("Backend:BaseUrl is not configured.");
    http.BaseAddress = new Uri(opts.BaseUrl!.TrimEnd('/') + "/");
})
.AddHttpMessageHandler<JwtSessionHandler>();

builder.Services.AddTransient<IUsersApiClient>(sp =>
    sp.GetRequiredService<HMS_Frontend.Services.Factories.IApiClientFactory>().CreateUsersClient());


// ===== General API Service =====
builder.Services.AddHttpClient<ApiService>((sp, http) =>
{
    var opts = sp.GetRequiredService<IOptions<BackendOptions>>().Value;
    if (string.IsNullOrWhiteSpace(opts.BaseUrl))
        throw new InvalidOperationException("Backend:BaseUrl is not configured.");
    http.BaseAddress = new Uri(opts.BaseUrl!.TrimEnd('/') + "/");
})
.AddHttpMessageHandler<JwtSessionHandler>();

builder.Services.AddTransient<IApiService>(sp =>
    sp.GetRequiredService<HMS_Frontend.Services.Factories.IApiClientFactory>().CreateApiService());

// Authentication
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/SignIn";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

// Pages
builder.Services.AddRazorPages();

var app = builder.Build();

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
app.UseSession();

app.MapRazorPages();
app.MapGet("/", context =>
{
    context.Response.Redirect("/Account/SignIn");
    return Task.CompletedTask;
});

app.Run();

public record BackendOptions { public string? BaseUrl { get; init; } }

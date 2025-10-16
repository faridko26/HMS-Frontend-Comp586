using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Bind Backend config
builder.Services.Configure<BackendOptions>(
    builder.Configuration.GetSection("Backend"));


var useFake = builder.Configuration.GetValue<bool>("Frontend:UseFakeApi");

if (useFake)
{
    builder.Services.AddSingleton<IAuthApiClient, FakeAuthApiClient>();
}
else
{
    builder.Services.AddHttpClient<AuthApiClient>((sp, http) =>
    {
        var opts = sp.GetRequiredService<IOptions<BackendOptions>>().Value;
        http.BaseAddress = new Uri(opts.BaseUrl!.TrimEnd('/') + "/");
    });
    builder.Services.AddScoped<IAuthApiClient, AuthApiClient>();
}




// Typed HttpClient for your API


builder.Services.AddRazorPages();
builder.Services.AddSession(); // optional: to hold tokens
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

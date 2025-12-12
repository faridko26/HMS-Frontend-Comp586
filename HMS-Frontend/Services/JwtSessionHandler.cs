// Services/JwtSessionHandler.cs
using System.Net.Http.Headers;

public class JwtSessionHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _accessor;
    public JwtSessionHandler(IHttpContextAccessor accessor) => _accessor = accessor;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var token = _accessor.HttpContext?.Session.GetString("JwtToken");
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, ct);
    }
}

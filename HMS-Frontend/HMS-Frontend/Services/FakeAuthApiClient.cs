// Services/FakeAuthApiClient.cs
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class FakeAuthApiClient : IAuthApiClient
{
    public Task<HttpResponseMessage> LoginAsync(LoginRequest req)
    {
        var ok = !string.IsNullOrWhiteSpace(req.Email)
                 && req.Email.Contains("@")
                 && req.Password == "Passw0rd!";
        return Task.FromResult(new HttpResponseMessage(ok ? HttpStatusCode.OK : HttpStatusCode.Unauthorized)
        {
            Content = new StringContent(ok ? "{\"token\":\"fake-jwt\"}" : "", Encoding.UTF8, "application/json")
        });
    }

    
}

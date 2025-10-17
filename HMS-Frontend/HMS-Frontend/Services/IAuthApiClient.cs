// Services/IAuthApiClient.cs
using System.Net.Http;
using System.Threading.Tasks;

public interface IAuthApiClient
{
    Task<HttpResponseMessage> LoginAsync(LoginRequest req);
    
}

//Add an interface so we can inject a fake
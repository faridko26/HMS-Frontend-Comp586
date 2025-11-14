using System.Threading.Tasks;

public interface IAuthApiClient
{
 
    Task<string?> LoginAsync(LoginRequest req);
}

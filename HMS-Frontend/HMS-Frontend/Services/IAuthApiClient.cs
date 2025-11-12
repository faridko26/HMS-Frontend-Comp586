using System.Threading.Tasks;

public interface IAuthApiClient
{
    /// <summary>
    /// Sends login request to backend and returns JWT token if successful, otherwise null.
    /// </summary>
    Task<string?> LoginAsync(LoginRequest req);
}

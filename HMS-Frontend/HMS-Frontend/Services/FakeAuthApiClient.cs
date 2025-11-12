using System.Threading.Tasks;

public class FakeAuthApiClient : IAuthApiClient
{
    public Task<string?> LoginAsync(LoginRequest req)
    {
        // Simulate simple fake login logic
        var ok = !string.IsNullOrWhiteSpace(req.Username)
                 && req.Password == "123456";

        // Return a fake JWT token if credentials are "valid"
        var fakeToken = ok
            ? "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
              "eyJ1c2VybmFtZSI6IiIgLCJyb2xlIjoiYWRtaW4ifQ." +
              "fakeSignature123456"
            : null;

        return Task.FromResult(fakeToken);
    }
}

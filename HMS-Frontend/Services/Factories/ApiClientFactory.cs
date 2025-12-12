using Microsoft.Extensions.Options;
using HMS_Frontend.Services;

namespace HMS_Frontend.Services.Factories
{
    public interface IApiClientFactory
    {
        IAuthApiClient CreateAuthClient();
        IUsersApiClient CreateUsersClient();
        IApiService CreateApiService();
    }

    public class ApiClientFactory : IApiClientFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public ApiClientFactory(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public IAuthApiClient CreateAuthClient()
        {
            var useFake = _configuration.GetValue<bool>("Frontend:UseFakeApi");
            if (useFake)
            {
                return _serviceProvider.GetRequiredService<FakeAuthApiClient>();
            }
            return _serviceProvider.GetRequiredService<AuthApiClient>();
        }

        public IUsersApiClient CreateUsersClient()
        {
            return _serviceProvider.GetRequiredService<UsersApiClient>();
        }

        public IApiService CreateApiService()
        {
            return _serviceProvider.GetRequiredService<ApiService>();
        }
    }
}

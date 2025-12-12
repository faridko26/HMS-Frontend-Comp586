// Services/IUsersApiClient.cs
using HMS_Frontend.Models;
public interface IUsersApiClient
{
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(int id);
    Task<bool> CreateAsync(CreateUserRequest req);
    Task<bool> UpdateAsync(int id, UpdateUserRequest req);
    Task<bool> DeactivateAsync(int id);

}



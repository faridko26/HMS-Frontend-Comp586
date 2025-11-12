// Services/IUsersApiClient.cs
public interface IUsersApiClient
{
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<bool> CreateAsync(CreateUserRequest req);
    Task<bool> UpdateAsync(Guid id, UpdateUserRequest req);
    Task<bool> DeactivateAsync(Guid id);
}



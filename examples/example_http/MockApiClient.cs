namespace WorkflowRestExample;

 public record UserDto(int Id, string Name, string Email);
public record UserDetailsDto(int Id, string Name, string Email, string Role, DateTime LastLogin);
public record UpdateRoleRequest(string NewRole);
public record UpdateRoleResponse(bool Success, DateTime UpdatedAt);

public class MockApiClient
{
  private readonly Dictionary<int, UserDetailsDto> _userDatabase = new Dictionary<int, UserDetailsDto>
  {
      {
          1, new UserDetailsDto(
              1, "John Doe", "john@example.com",
              "User", DateTime.UtcNow.AddDays(-5)
          )
      }
  };
  private readonly Random _random = new Random();

  public async Task<UserDto> GetUserAsync(int userId)
  {
    await Task.Delay(200); // Simulate network delay
            
    if (!_userDatabase.TryGetValue(userId, out UserDetailsDto? user))
      throw new HttpRequestException("User not found", null, System.Net.HttpStatusCode.NotFound);

    return new UserDto(user.Id, user.Name, user.Email);
  }

  public async Task<UserDetailsDto> GetUserDetailsAsync(int userId)
  {
    await Task.Delay(300); // Simulate network delay
            
    if (!_userDatabase.ContainsKey(userId))
      throw new HttpRequestException("User not found", null, System.Net.HttpStatusCode.NotFound);

    return _userDatabase[userId];
  }

  public async Task<UpdateRoleResponse> UpdateUserRoleAsync(int userId, UpdateRoleRequest request)
  {
    await Task.Delay(500); // Simulate network delay

    if (!_userDatabase.ContainsKey(userId))
      throw new HttpRequestException("User not found", null, System.Net.HttpStatusCode.NotFound);

    if (string.IsNullOrEmpty(request.NewRole))
      throw new HttpRequestException("Invalid role", null, System.Net.HttpStatusCode.BadRequest);

    var user = _userDatabase[userId];
    _userDatabase[userId] = user with { Role = request.NewRole };

    return new UpdateRoleResponse(true, DateTime.UtcNow);
  }
}

using WorkflowFramework;
namespace WorkflowRestExample;

public class UserManagementWorkflow(MockApiClient apiClient)
{
  private readonly Workflow _workflow = new Workflow();
  public async Task<string> PromoteUserToAdmin(int userId)
  {
    // Step 1: Validate user exists and get basic info
    var validateUser = _workflow.CreateStep<int, UserDto>(
        "Validate User",
         id => new UserDto(id, "John Doe", "john.doe@example.com"));
     

    // Step 2: Get detailed user information
    var getUserDetails = new GetUserDetailsStep();

    // Step 3: Update user role
    var updateRole = _workflow.CreateStep<UserDetailsDto, Task<UpdateRoleResponse>>(
        "Update Role", async details => await apiClient.UpdateUserRoleAsync(details.Id, new UpdateRoleRequest("Admin")));
        

    // Step 4: Generate audit log
    var generateAuditLog = _workflow.CreateStep<(UserDetailsDto Details, UpdateRoleResponse Update), string>(
        "Generate Audit Log",
        data =>
        {
          var (details, update) = data;
          return $"User {details.Name} (ID: {details.Id}) promoted to Admin role at {update.UpdatedAt:yyyy-MM-dd HH:mm:ss UTC}. " +
                 $"Previous role was {details.Role}.";
        });

    try
    {
      // Execute workflow
      var userInfo = validateUser.Execute(userId);
      var userDetails = await getUserDetails.Execute(userInfo);
      var updateResult =  await updateRole.Execute(userDetails);
      var auditLog = generateAuditLog.Execute((userDetails, updateResult));

      // Print execution history
      foreach (var step in _workflow.GetExecutionHistory())
      {
        var duration = step.CompletedAt.HasValue && step.StartedAt.HasValue
            ? (step.CompletedAt.Value - step.StartedAt.Value).TotalSeconds
            : 0;

        Console.WriteLine($"\nStep: {step.Name}");
        Console.WriteLine($"Status: {step.Status}");
        Console.WriteLine($"Duration: {duration:F2}s");
      }

      return auditLog;
    }
    catch (Exception ex)
    {
      _workflow.LogFailing();
            throw;
        }
  }
}
public class GetUserDetailsStep
{
  private const string StepName = "Get User Details";
  public static WorkflowStep<UserDto, Task<UserDetailsDto>> GetStep(Workflow workflow, MockApiClient apiClient)
  {
    return workflow.CreateStep<UserDto, Task<UserDetailsDto>>(
        StepName,
        async user =>
        {
          var details = await apiClient.GetUserDetailsAsync(user.Id);
          if (details.Role == "Admin")
            throw new Exception($"User {user.Id} is already an Admin.");
          return details;
        });
  }
}

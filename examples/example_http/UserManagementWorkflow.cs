using WorkflowFramework;
namespace WorkflowRestExample;

public class UserManagementWorkflow(MockApiClient apiClient) : Workflow
{
  public async Task<string> PromoteUserToAdmin(int userId)
  {
    // Step 1: Validate user exists and get basic info
    var validateUser = CreateStep<int, UserDto>(
        "Validate User",
        async id => await apiClient.GetUserAsync(id));

    // Step 2: Get detailed user information - defining a step in its own class
    var getUserDetails = GetUserDetailsStep.GetStep(this, apiClient);

    // Step 3: Update user role
    var updateRole = CreateStep<UserDetailsDto, UpdateRoleResponse>(
        "Update Role", async details => await apiClient.UpdateUserRoleAsync(details.Id, new UpdateRoleRequest("Admin")));
        

    // Step 4: Generate audit log
    var generateAuditLog = CreateStep<(UserDetailsDto Details, UpdateRoleResponse Update), string>(
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
      // var userInfo = await validateUser.Execute(userId);
      // var userDetails = await getUserDetails.Execute(userInfo);
      // var updateResult =  await updateRole.Execute(userDetails);
      // var auditLog = await generateAuditLog.Execute((userDetails, updateResult));

      var userInfo = await ExecuteStep(validateUser, userId);
      var userDetails = await ExecuteStep(getUserDetails, userInfo);
      var updateResult = await ExecuteStep(updateRole, userDetails);
      var auditLog = await ExecuteStep(generateAuditLog, (userDetails, updateResult));
      Complete();

      return auditLog;
    }
    catch (Exception)
    {
      LogHistory();
    }
    return null;
  }
}
public static class GetUserDetailsStep
{
  private const string StepName = "Get User Details";
  public static WorkflowStep<UserDto, UserDetailsDto> GetStep(Workflow workflow, MockApiClient apiClient)
  {
    return Workflow.CreateStep<UserDto, UserDetailsDto>(
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

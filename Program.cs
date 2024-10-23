using WorkflowRestExample;
var apiClient = new MockApiClient();
var workflow = new UserManagementWorkflow(apiClient);

try
{
  // Test successful promotion
  var result = await workflow.PromoteUserToAdmin(1);
  Console.WriteLine("\nSuccess!");
  Console.WriteLine(result);

  // Test failure case (user not found)
  result = await workflow.PromoteUserToAdmin(999);
  Console.WriteLine(result);
}
catch (Exception ex)
{
  Console.WriteLine($"\nError: {ex.Message}");
}

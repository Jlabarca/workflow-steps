using WorkflowRestExample;

var apiClient = new MockApiClient();
var workflow = new UserManagementWorkflow(apiClient);

  // Test successful promotion
  var result = await workflow.PromoteUserToAdmin(1);
  Console.WriteLine(result);

  // Test failure case (user not found)
  workflow = new UserManagementWorkflow(apiClient);
  result = await workflow.PromoteUserToAdmin(999);
  Console.WriteLine(result);

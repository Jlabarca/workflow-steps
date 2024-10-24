// namespace WorkflowFramework;
//
// public class ExampleWorkflow
// {
//   public static void RunExample()
//   {
//     var workflow = new Workflow();
//
//     // Define steps
//     var parseInput = workflow.CreateStep<string, Dictionary<string, string[]>>(
//         "Parse Input",
//         input => new Dictionary<string, string[]>
//         {
//             ["parsed"] = input.Split(',')
//         });
//
//     var processData = workflow.CreateStep<Dictionary<string, string[]>, Dictionary<string, string[]>>(
//         "Process Data",
//         data => new Dictionary<string, string[]>
//         {
//             ["processed"] = Array.ConvertAll(data["parsed"], s => s.ToUpper())
//         });
//
//     var formatOutput = workflow.CreateStep<Dictionary<string, string[]>, string>(
//         "Format Output",
//         data => string.Join("|", data["processed"]));
//
//     try
//     {
//       // Execute workflow
//       var inputData = "a,b,c";
//       var parsed = parseInput.Execute(inputData);
//       var processed = processData.Execute(parsed);
//       var result = formatOutput.Execute(processed);
//
//       Console.WriteLine($"Result: {result}"); // Output: Result: A|B|C
//
//       // Print execution history
//       foreach (var step in workflow.GetExecutionHistory())
//       {
//         Console.WriteLine($"\nStep: {step.Name}");
//         Console.WriteLine($"Status: {step.Status}");
//         Console.WriteLine($"Input: {step.InputData}");
//         Console.WriteLine($"Output: {step.OutputData}");
//         Console.WriteLine($"Duration: {(step.CompletedAt - step.StartedAt)?.TotalSeconds:F2}s");
//       }
//     }
//     catch (Exception ex)
//     {
//       Console.WriteLine($"Workflow failed: {ex.Message}");
//     }
//   }
//
//   // Example with branching
//   public static void RunBranchingExample()
//   {
//     var workflow = new Workflow();
//
//     var validate = workflow.CreateStep<string, int>(
//         "Validate",
//         input =>
//         {
//           if (!int.TryParse(input, out var number))
//             throw new ArgumentException("Invalid input");
//           return number;
//         });
//
//     var processEven = workflow.CreateStep<int, int>(
//         "Process Even",
//         num => num * 2);
//
//     var processOdd = workflow.CreateStep<int, int>(
//         "Process Odd",
//         num => num * 3);
//
//     try
//     {
//       var inputData = "42";
//       var num = validate.Execute(inputData);
//       var result = num % 2 == 0
//           ? processEven.Execute(num)
//           : processOdd.Execute(num);
//
//       Console.WriteLine($"Result: {result}");
//     }
//     catch (Exception ex)
//     {
//       Console.WriteLine($"Workflow failed: {ex.Message}");
//     }
//   }
//
//   // Example with async execution
//   public static async Task RunAsyncExample()
//   {
//     var workflow = new Workflow();
//
//     var asyncStep = workflow.CreateStep<string, Task<string>>(
//         "Async Processing",
//         async input =>
//         {
//           await Task.Delay(1000); // Simulate async work
//           return input.ToUpper();
//         });
//
//     try
//     {
//       var result = await asyncStep.Execute("hello");
//       Console.WriteLine($"Async Result: {result}");
//     }
//     catch (Exception ex)
//     {
//       Console.WriteLine($"Async workflow failed: {ex.Message}");
//     }
//   }
// }

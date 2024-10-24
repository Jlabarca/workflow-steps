using WorkflowFramework;

public class WorkflowStep<TInput, TOutput>
{
    public string StepId { get; }
    public string Name { get; }
    public StepState State { get; }
    private readonly Delegate _execution;

    public WorkflowStep(string name, Delegate execution, TimeSpan? timeout = null)
    {
        StepId = Guid.NewGuid().ToString();
        Name = name;
        _execution = execution;
        State = new StepState(StepId, name, timeout);
    }

    public async Task<TOutput> Execute(TInput input)
    {
        State.Status = StepStatus.InProgress;
        State.StartedAt = DateTime.UtcNow;

        try
        {
            TOutput result;
            if (_execution is Func<TInput, Task<TOutput>> asyncFunc)
            {
                using var cts = new CancellationTokenSource();
                if (State.Timeout.HasValue)
                {
                    cts.CancelAfter(State.Timeout.Value);
                }

                try
                {
                    var executionTask = asyncFunc(input);
                    result = await ExecuteWithTimeout(executionTask, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    throw new TimeoutException($"Step '{Name}' timed out after {State.Timeout.Value.TotalSeconds} seconds");
                }
            }
            else
            {
                result = ((Func<TInput, TOutput>)_execution)(input);
            }

            State.OutputData = result;
            State.Status = StepStatus.Completed;
            return result;
        }
        catch (Exception ex)
        {
            State.Error = ex;
            State.Status = StepStatus.Failed;
            throw;
        }
        finally
        {
            State.CompletedAt = DateTime.UtcNow;
            Console.WriteLine(State);
        }
    }

    private async Task<TOutput> ExecuteWithTimeout(Task<TOutput> task, CancellationToken cancellationToken)
    {
        if (State.Timeout == null) return await task;

        var completedTask = await Task.WhenAny(task, Task.Delay(State.Timeout.Value, cancellationToken));
        if (completedTask == task)
        {
            return await task; // Unwrap the result
        }

        throw new TimeoutException($"Step '{Name}' exceeded timeout of {State.Timeout.Value.TotalSeconds} seconds");
    }
}

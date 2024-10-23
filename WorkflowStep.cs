using WorkflowFramework;
public class WorkflowStep<TInput, TOutput>
{
    public string StepId { get; }
    public string Name { get; }
    private readonly Delegate _execution;
    public StepState State { get; }

    public WorkflowStep(string name, Delegate execution)
    {
        StepId = Guid.NewGuid().ToString();
        Name = name;
        _execution = execution;
        State = new StepState(StepId, name);
    }

    public TOutput Execute(TInput input)
    {
        if (_execution is Func<TInput, Task<TOutput>> asyncFunc)
        {
            throw new InvalidOperationException("This step is async. Use ExecuteAsync instead.");
        }

        var func = (Func<TInput, TOutput>)_execution;
        State.Status = StepStatus.InProgress;
        State.InputData = input;
        State.StartedAt = DateTime.UtcNow;

        try
        {
            var result = func(input);
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
        }
    }
}

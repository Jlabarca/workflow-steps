using WorkflowRestExample;
namespace WorkflowFramework;

public class Workflow
{
    private readonly List<StepState> _history = new();
    private readonly Dictionary<string, object> _steps = new();

    public WorkflowStep<TInput, TOutput> CreateStep<TInput, TOutput>(
            string name,
            Func<TInput, TOutput> execution)
    {
        var step = new WorkflowStep<TInput, TOutput>(name, execution);
        _steps[step.StepId] = step;
        _history.Add(step.State);
        return step;
    }

    public WorkflowStep<TInput, TOutput> AddStep<TInput, TOutput>(
            string name,
            Func<TInput, TOutput> execution)
    {
        var step = new WorkflowStep<TInput, TOutput>(name, execution);
        _steps[step.StepId] = step;
        _history.Add(step.State);
        return step;
    }

    public StepState? GetStepState(string stepId)
    {
        return _steps.TryGetValue(stepId, out var step)
                ? ((dynamic)step).State 
                : null;
    }

    public IReadOnlyList<StepState> GetExecutionHistory()
    {
        return _history.AsReadOnly();
    }

    public void LogFailing()
    {
        Console.WriteLine("History ");
        int i = 0;
        foreach (var step in _history)
        {
            Console.WriteLine($"{++i}: {step.Name}|Status: {step.Status}|Duration: {step.Duration:F2}s"); 
        }
    }
}


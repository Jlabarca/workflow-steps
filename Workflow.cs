namespace WorkflowFramework;

public class Workflow
{
  private readonly Guid _id = Guid.NewGuid();
  private readonly string _name;
  private readonly List<StepState> _history = [];

  public TimeSpan Duration { get; private set; }
  private DateTime _startedAt;
  private DateTime _completedAt;
  private bool _started = false;

  protected Workflow()
  {
    _name = GetType().Name;
  }

  public static WorkflowStep<TInput, TOutput> CreateStep<TInput, TOutput>(
      string name,
      Func<TInput, TOutput> execution,
      TimeSpan? timeout = null) => new WorkflowStep<TInput, TOutput>(name, execution, timeout);


  public static WorkflowStep<TInput, TOutput> CreateStep<TInput, TOutput>(
      string name,
      Func<TInput, Task<TOutput>> execution,
      TimeSpan? timeout = null) => new WorkflowStep<TInput, TOutput>(name, execution, timeout);

 public async Task<TOutput> ExecuteStep<TInput, TOutput>(
      WorkflowStep<TInput, TOutput> step,
      TInput input,
      TimeSpan? timeout = null)
  {
    if (!_started) 
    {
        _startedAt = DateTime.UtcNow;
        _started = true;
    }
    _history.Add(step.State);
    return await step.Execute(input);
  }

  public async Task ExecuteStep<TInput, TOutput>(string name,
      Func<TInput, Task<TOutput>> execution,
      TInput input,
      TimeSpan? timeout = null)
  {
    if (!_started)
    {
      _startedAt = DateTime.UtcNow;
      _started = true;
    }
    var step = new WorkflowStep<TInput, TOutput>(name, execution, timeout);
    _history.Add(step.State);
    await step.Execute(input);
  }

    public void Complete()
    {
        _completedAt = DateTime.UtcNow;
        Duration = _completedAt - _startedAt;
        Console.WriteLine($"{_name} completed in {Duration.TotalMilliseconds:N2}ms");
        LogHistory();
    }

    public IReadOnlyList<StepState> GetExecutionHistory()
  {
    return _history.AsReadOnly();
  }

  public void LogHistory()
  {
    int i = 0;
    foreach (var step in _history)
    {
      Console.WriteLine($"{++i}. {step}");
    }
  }
}

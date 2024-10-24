namespace WorkflowFramework;

public enum StepStatus
{
  Pending,
  InProgress,
  Completed,
  Failed,
  Skipped
}

public class StepState(string stepId, string name, TimeSpan? timeout = null)
{
    public string StepId { get; } = stepId;
    public string Name { get; } = name;
    public StepStatus Status { get; set; } = StepStatus.Pending;
    public object InputData { get; set; }
  public object OutputData { get; set; }
    public TimeSpan? Timeout { get; init; } = timeout;
    public DateTime? StartedAt { get; set; }
  public DateTime? CompletedAt { get; set; }
  public Exception? Error { get; set; }
  public double DurationInMs => CompletedAt.HasValue && StartedAt.HasValue ? (CompletedAt.Value - StartedAt.Value).TotalMilliseconds : 0;

    public override string ToString()
  {
    return Error != null ? 
      $"|| {Status} || {Name} || Duration: {DurationInMs}ms || Error: {Error.Message}"
      : $"|| {Status} || {Name} || Duration: {DurationInMs:N2}ms";
  }
}

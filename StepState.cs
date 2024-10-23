namespace WorkflowFramework;

public enum StepStatus
{
  Pending,
  InProgress,
  Completed,
  Failed,
  Skipped
}

public class StepState
{
  public string StepId { get; }
  public string Name { get; }
  public StepStatus Status { get; set; }
  public object InputData { get; set; }
  public object OutputData { get; set; }
  public DateTime? StartedAt { get; set; }
  public DateTime? CompletedAt { get; set; }
  public Exception Error { get; set; }
  public double Duration => CompletedAt.HasValue && StartedAt.HasValue ? (CompletedAt.Value - StartedAt.Value).TotalSeconds : 0;

  public StepState(string stepId, string name)
  {
    StepId = stepId;
    Name = name;
    Status = StepStatus.Pending;
  }
}

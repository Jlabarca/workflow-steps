using System.Collections.Concurrent;

namespace WorkflowFramework;

public class WorkflowAnalytics
{
    public Guid WorkflowId { get; }
    public string WorkflowName { get; }
    public DateTime CreatedAt { get; }
    public DateTime? CompletedAt { get; private set; }
    public TimeSpan? Duration => CompletedAt - CreatedAt;
    
    private readonly List<StepState> _history;
    private readonly ConcurrentDictionary<string, int> _stepExecutionCounts = new();
    private readonly ConcurrentDictionary<string, List<TimeSpan>> _stepDurations = new();

    public WorkflowAnalytics(Guid workflowId, string workflowName, List<StepState> history)
    {
        WorkflowId = workflowId;
        WorkflowName = workflowName;
        CreatedAt = DateTime.UtcNow;
        _history = history;
    }

    public void UpdateAnalytics()
    {
        if (_history.Count == 0) return;

        CompletedAt = DateTime.UtcNow;
        UpdateStepMetrics();
    }

    private void UpdateStepMetrics()
    {
        foreach (var step in _history)
        {
            // Update execution counts
            _stepExecutionCounts.AddOrUpdate(
                step.Name,
                1,
                (_, count) => count + 1
            );

            // Update duration metrics if step has completed
            if (step.StartedAt.HasValue && step.CompletedAt.HasValue)
            {
                var duration = step.CompletedAt.Value - step.StartedAt.Value;
                _stepDurations.AddOrUpdate(
                    step.Name,
                    new List<TimeSpan> { duration },
                    (_, durations) =>
                    {
                        durations.Add(duration);
                        return durations;
                    }
                );
            }
        }
    }

    public WorkflowMetrics GetMetrics()
    {
        var stepMetrics = new List<StepMetrics>();
        var allSteps = _history.GroupBy(x => x.Name);

        foreach (var stepGroup in allSteps)
        {
            var stepName = stepGroup.Key;
            var steps = stepGroup.ToList();
            var executionCount = steps.Count;
            var successCount = steps.Count(s => s.Status == StepStatus.Completed);
            var failureCount = steps.Count(s => s.Status == StepStatus.Failed);
            var avgDuration = _stepDurations.TryGetValue(stepName, out var durations)
                ? TimeSpan.FromMilliseconds(durations.Average(d => d.TotalMilliseconds))
                : TimeSpan.Zero;

            stepMetrics.Add(new StepMetrics(
                Name: stepName,
                ExecutionCount: executionCount,
                SuccessRate: executionCount > 0 ? (double)successCount / executionCount : 0,
                FailureRate: executionCount > 0 ? (double)failureCount / executionCount : 0,
                AverageDuration: avgDuration,
                MinDuration: durations?.Min() ?? TimeSpan.Zero,
                MaxDuration: durations?.Max() ?? TimeSpan.Zero
            ));
        }

        return new WorkflowMetrics(
            WorkflowId,
            WorkflowName,
            CreatedAt,
            CompletedAt,
            Duration ?? TimeSpan.Zero,
            stepMetrics,
            _history.Count,
            _history.Count(s => s.Status == StepStatus.Completed),
            _history.Count(s => s.Status == StepStatus.Failed)
        );
    }
}

public record WorkflowMetrics(
    Guid WorkflowId,
    string WorkflowName,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    TimeSpan TotalDuration,
    List<StepMetrics> StepMetrics,
    int TotalSteps,
    int SuccessfulSteps,
    int FailedSteps)
{
    public double SuccessRate => TotalSteps > 0 ? (double)SuccessfulSteps / TotalSteps : 0;
    public double FailureRate => TotalSteps > 0 ? (double)FailedSteps / TotalSteps : 0;
}

public record StepMetrics(
    string Name,
    int ExecutionCount,
    double SuccessRate,
    double FailureRate,
    TimeSpan AverageDuration,
    TimeSpan MinDuration,
    TimeSpan MaxDuration);


namespace UmbLink.Application.Interfaces;

public interface IBackgroundTaskQueue
{
    void Enqueue(Func<IServiceProvider, CancellationToken, Task> task);
    Task<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken ct);
}

using System.Threading.Channels;
using UmbLink.Application.Interfaces;

namespace UmbLink.Application.BackgroundServices;

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<IServiceProvider, CancellationToken, Task>> _queue =
        Channel.CreateUnbounded<Func<IServiceProvider, CancellationToken, Task>>();

    public void Enqueue(Func<IServiceProvider, CancellationToken, Task> task) =>
        _queue.Writer.TryWrite(task);

    public async Task<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken ct) =>
        await _queue.Reader.ReadAsync(ct);
}

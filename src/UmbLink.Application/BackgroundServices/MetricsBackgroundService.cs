using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UmbLink.Application.Interfaces;

namespace UmbLink.Application.BackgroundServices;

public class MetricsBackgroundService(
    IBackgroundTaskQueue queue,
    IServiceProvider sp,
    ILogger<MetricsBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var task = await queue.DequeueAsync(ct);
                await task(sp, ct);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao processar tarefa de métricas");
            }
        }
    }
}

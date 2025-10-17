using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ETLProject.ExceptionHandling;

public class ExceptionHandlingDecorator : IHostedService
{
    private readonly IHostedService _decorated;
    private readonly ILogger<ExceptionHandlingDecorator> _logger;
    private readonly IHostApplicationLifetime _appLifetime;

    public ExceptionHandlingDecorator(IHostedService decorated,ILogger<ExceptionHandlingDecorator> logger,
        IHostApplicationLifetime appLifetime)
    {
        _decorated = decorated;
        _logger = logger;
        _appLifetime = appLifetime;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _decorated.StartAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred during the hosted service execution. The application will now stop.");
        }
        finally
        {
            _appLifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _decorated.StopAsync(cancellationToken);
    }
}

using ETLProject.ExceptionHandling;
using ETLProject;
using ETLProject.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using ETLProject.Processor;
using ETLProject.Core.Repositories;
using ETLProject.Infrastructure.Repository;
using ETLProject.Core.Contracts;

//Code here I registered all dependencies in DI container and run the app
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.ClearProviders();
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();
        builder.Logging.AddSerilog(logger);

        builder.Services.AddSingleton<CsvTaxiTripReader>();
        builder.Services.AddSingleton<ITaxiTripProcessor, TaxiTripProcessor>();
        builder.Services.AddScoped<ITaxiTripRepository, TaxiTripRepository>();

        builder.Services.AddSingleton<EtlOrchestrator>();

        builder.Services.AddSingleton<IHostedService>(provider =>
        {
            var orchestrator = provider.GetRequiredService<EtlOrchestrator>();

            var decoratorLogger = provider.GetRequiredService<ILogger<ExceptionHandlingDecorator>>();
            var appLifetime = provider.GetRequiredService<IHostApplicationLifetime>();

            return new ExceptionHandlingDecorator(orchestrator, decoratorLogger, appLifetime);
        });

        var app = builder.Build();

        try
        {
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Logger.Fatal(ex, "The application terminated unexpectedly during startup.");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}

using ETLProject.Core.Contracts;
using ETLProject.Core.Repositories;
using ETLProject.Infrastructure;
using ETLProject.Processor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ETLProject;

//The purpose of this class is to configure the overall procces of the app.
public class EtlOrchestrator : IHostedService
{
    private readonly ILogger<EtlOrchestrator> _logger;
    private readonly CsvTaxiTripReader _reader;
    private readonly ITaxiTripProcessor _processor;
    private readonly ITaxiTripRepository _repository;
    private readonly IConfiguration _config;

    public EtlOrchestrator(ILogger<EtlOrchestrator> logger, CsvTaxiTripReader reader,
        ITaxiTripProcessor processor, ITaxiTripRepository repository,IConfiguration config)
    {
        _logger = logger;
        _reader = reader;
        _processor = processor;
        _repository = repository;
        _config = config;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ETL process starting.");

        var inputPath = _config["FilePaths:InputCsv"];
        var duplicatesPath = _config["FilePaths:DuplicatesCsv"];

        _logger.LogInformation("Extracting data from {path}...", inputPath);
        var rawTrips = _reader.ReadTripsFromUrlAsync(inputPath!);

        _logger.LogInformation("Transforming data...");
        var processingResult = await _processor.ProcessAsync(rawTrips);
        _logger.LogInformation(
            "Transformation complete. Found {validCount} valid trips and {duplicateCount} duplicates.",
            processingResult.ValidTrips.Count,
            processingResult.DuplicateTrips.Count);

        await _repository.SaveTripsAsync(processingResult.ValidTrips);
        await _repository.WriteDuplicatesAsync(processingResult.DuplicateTrips, duplicatesPath!);

        _logger.LogInformation(
            "ETL process finished successfully. Final row count in table: {count}",
            processingResult.ValidTrips.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
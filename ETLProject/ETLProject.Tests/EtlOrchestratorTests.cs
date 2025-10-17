using ETLProject.Core.Models;
using ETLProject.Core.Repositories;
using ETLProject.Infrastructure;
using ETLProject.Processor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace ETLProject.Tests;

public class EtlOrchestratorTests
{
    [Fact]
    public async Task StartAsync_ShouldCallReaderProcessorAndRepository()
    {
        // Arrange
        var logger = Moq.Mock.Of<ILogger<EtlOrchestrator>>();
        var readerMock = new Moq.Mock<CsvTaxiTripReader>();
        var processorMock = new Moq.Mock<Core.Contracts.ITaxiTripProcessor>();
        var repoMock = new Moq.Mock<ITaxiTripRepository>();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["FilePaths:InputCsv"] = "test.csv",
            ["FilePaths:DuplicatesCsv"] = "duplicates.csv"
        }).Build();

        readerMock.Setup(r => r.ReadTripsFromUrlAsync(Moq.It.IsAny<string>()))
                  .Returns(EmptyAsync());

        processorMock.Setup(p => p.ProcessAsync(It.IsAny<IAsyncEnumerable<TaxiTrip>>()))
             .Returns(Task.FromResult(new ProcessingResult(new List<TaxiTrip>(), new List<TaxiTrip>())));

        var orchestrator = new EtlOrchestrator(logger, readerMock.Object, processorMock.Object, repoMock.Object, config);

        // Act
        await orchestrator.StartAsync(default);

        // Assert
        processorMock.Verify(p => p.ProcessAsync(Moq.It.IsAny<IAsyncEnumerable<TaxiTrip>>()), Moq.Times.Once);
        repoMock.Verify(r => r.SaveTripsAsync(Moq.It.IsAny<IEnumerable<TaxiTrip>>()), Moq.Times.Once);
    }

    private async IAsyncEnumerable<TaxiTrip> EmptyAsync() { yield break; await Task.Yield(); }
}

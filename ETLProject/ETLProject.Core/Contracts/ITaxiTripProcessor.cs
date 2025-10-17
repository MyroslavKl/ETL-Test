using ETLProject.Core.Models;
using ETLProject.Processor;

namespace ETLProject.Core.Contracts;

public interface ITaxiTripProcessor
{
    Task<ProcessingResult> ProcessAsync(IAsyncEnumerable<TaxiTrip> trips);
}

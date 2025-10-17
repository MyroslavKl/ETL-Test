using ETLProject.Core.Models;

namespace ETLProject.Core.Repositories;

public interface ITaxiTripRepository
{
    Task SaveTripsAsync(IEnumerable<TaxiTrip> trips);
    Task WriteDuplicatesAsync(IEnumerable<TaxiTrip> duplicates, string filePath);
}

using CsvHelper;
using ETLProject.Core.Models;
using ETLProject.Infrastructure.Models;
using System.Globalization;

namespace ETLProject.Infrastructure;

//I use this class to not load all records to memory, it loads line by line.
public class CsvTaxiTripReader
{
    private readonly HttpClient _httpClient;

    public CsvTaxiTripReader()
    {
        _httpClient = new HttpClient();
    }

    public async IAsyncEnumerable<TaxiTrip> ReadTripsFromUrlAsync(string url)
    {
        var responseStream = await _httpClient.GetStreamAsync(url);

        using var reader = new StreamReader(responseStream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<TaxiTripMap>();

        await foreach (var record in csv.GetRecordsAsync<TaxiTrip>())
        {
            yield return record;
        }
    }
}

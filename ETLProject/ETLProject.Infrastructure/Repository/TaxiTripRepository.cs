using CsvHelper;
using ETLProject.Core.Models;
using ETLProject.Core.Repositories;
using ETLProject.Infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Diagnostics;
using System.Globalization;

namespace ETLProject.Infrastructure.Repository;

//Code here configure connection to db and savind the duplicates into another csv file
public class TaxiTripRepository : ITaxiTripRepository
{
    private readonly string _connectionString;
    private readonly ILogger<TaxiTripRepository> _logger;

    public TaxiTripRepository(IConfiguration configuration, ILogger<TaxiTripRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string is not found in configuration or doesn't exist.");
        _logger = logger;
    }

    public async Task SaveTripsAsync(IEnumerable<TaxiTrip> trips)
    {
        var tripsList = trips.ToList();
        if (!tripsList.Any())
        {
            _logger.LogWarning("SaveTripsAsync was called with no trips to save.");
            return;
        }

        _logger.LogInformation("Starting bulk insert of {TripCount} trips.", tripsList.Count);

        await BulkInsertAsync(tripsList);
    }

    private async Task BulkInsertAsync(List<TaxiTrip> tripsList)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = "TaxiTripData",
                BatchSize = 5000
            };

            var dataTable = CreateTripsDataTable(tripsList);
            MapColumns(bulkCopy.ColumnMappings);

            await bulkCopy.WriteToServerAsync(dataTable);

            stopwatch.Stop();
            _logger.LogInformation(
                "Successfully inserted {TripCount} trips in {ElapsedMilliseconds} ms.",
                tripsList.Count,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during bulk insert of {TripCount} records.", tripsList.Count);
            throw;
        }
    }

    public async Task WriteDuplicatesAsync(IEnumerable<TaxiTrip> duplicates, string filePath)
    {
        var duplicatesList = duplicates.ToList();
        _logger.LogInformation("Writing {DuplicateCount} duplicates to file: {FilePath}", duplicatesList.Count, filePath);

        if (!duplicatesList.Any())
        {
            _logger.LogInformation("No duplicates to write, skipping file creation.");
            return;
        }

        await WriteDuplicateAsync(duplicatesList, filePath);
    }

    private async Task WriteDuplicateAsync(List<TaxiTrip> duplicatesList,string filePath) {
        try
        {
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<TaxiTripMap>();
            await csv.WriteRecordsAsync(duplicatesList);
            _logger.LogInformation("Successfully wrote duplicates to file.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write duplicates to file: {FilePath}", filePath);
            throw;
        }
    }

    private DataTable CreateTripsDataTable(List<TaxiTrip> trips)
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add(nameof(TaxiTrip.TpepPickupDatetime), typeof(DateTime));
        dataTable.Columns.Add(nameof(TaxiTrip.TpepDropoffDatetime), typeof(DateTime));

        var passengerCountColumn = new DataColumn(nameof(TaxiTrip.PassengerCount), typeof(int));
        passengerCountColumn.AllowDBNull = true;
        dataTable.Columns.Add(passengerCountColumn);

        dataTable.Columns.Add(nameof(TaxiTrip.TripDistance), typeof(decimal));
        dataTable.Columns.Add(nameof(TaxiTrip.StoreAndFwdFlag), typeof(string));
        dataTable.Columns.Add(nameof(TaxiTrip.PULocationID), typeof(int));
        dataTable.Columns.Add(nameof(TaxiTrip.DOLocationID), typeof(int));
        dataTable.Columns.Add(nameof(TaxiTrip.FareAmount), typeof(decimal));
        dataTable.Columns.Add(nameof(TaxiTrip.TipAmount), typeof(decimal));

        foreach (var trip in trips)
        {
            dataTable.Rows.Add(
                trip.TpepPickupDatetime,
                trip.TpepDropoffDatetime,
                trip.PassengerCount.HasValue ? (object)trip.PassengerCount.Value : DBNull.Value,
                trip.TripDistance,
                trip.StoreAndFwdFlag,
                trip.PULocationID,
                trip.DOLocationID,
                trip.FareAmount,
                trip.TipAmount
            );
        }
        return dataTable;
    }

    private void MapColumns(SqlBulkCopyColumnMappingCollection mappings)
    {
        mappings.Add(nameof(TaxiTrip.TpepPickupDatetime), "tpep_pickup_datetime");
        mappings.Add(nameof(TaxiTrip.TpepDropoffDatetime), "tpep_dropoff_datetime");
        mappings.Add(nameof(TaxiTrip.PassengerCount), "passenger_count");
        mappings.Add(nameof(TaxiTrip.TripDistance), "trip_distance");
        mappings.Add(nameof(TaxiTrip.StoreAndFwdFlag), "store_and_fwd_flag");
        mappings.Add(nameof(TaxiTrip.PULocationID), "PULocationID");
        mappings.Add(nameof(TaxiTrip.DOLocationID), "DOLocationID");
        mappings.Add(nameof(TaxiTrip.FareAmount), "fare_amount");
        mappings.Add(nameof(TaxiTrip.TipAmount), "tip_amount");
    }
}

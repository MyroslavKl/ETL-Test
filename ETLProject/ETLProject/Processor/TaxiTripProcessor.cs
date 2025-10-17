using ETLProject.Core.Models;

namespace ETLProject.Processor;

public class TaxiTripProcessor
{
    private readonly TimeZoneInfo _timeZone;

    public TaxiTripProcessor()
    {
        try
        {
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        }
    }

    public async Task<ProcessingResult> ProcessAsync(IAsyncEnumerable<TaxiTrip> trips)
    {
        var seenTrips = new HashSet<TripKey>();
        var validRecords = new List<TaxiTrip>();
        var duplicateRecords = new List<TaxiTrip>();

        await foreach (var trip in trips)
        {
            ClassifyTrip(trip, seenTrips, validRecords, duplicateRecords);
        }

        return new ProcessingResult(validRecords, duplicateRecords);
    }

    private void ClassifyTrip(TaxiTrip trip,HashSet<TripKey> seenTrips,
        List<TaxiTrip> validRecords, List<TaxiTrip> duplicateRecords)
    {
        TransformRecord(trip);

        var key = new TripKey(trip.TpepPickupDatetime, trip.TpepDropoffDatetime, trip.PassengerCount);

        if (seenTrips.Add(key))
        {
            ConvertToUtc(trip);
            validRecords.Add(trip);
        }
        else
        {
            duplicateRecords.Add(trip);
        }
    }

    private void TransformRecord(TaxiTrip trip)
    {
        trip.StoreAndFwdFlag = (trip.StoreAndFwdFlag ?? string.Empty).Trim().ToUpper() switch
        {
            "N" => "No",
            "Y" => "Yes",
            _ => "N/A"
        };
    }

    private void ConvertToUtc(TaxiTrip trip)
    {
        var pickupTime = DateTime.SpecifyKind(trip.TpepPickupDatetime, DateTimeKind.Unspecified);
        var dropoffTime = DateTime.SpecifyKind(trip.TpepDropoffDatetime, DateTimeKind.Unspecified);

        trip.TpepPickupDatetime = TimeZoneInfo.ConvertTimeToUtc(pickupTime, _timeZone);
        trip.TpepDropoffDatetime = TimeZoneInfo.ConvertTimeToUtc(dropoffTime, _timeZone);
    }

}

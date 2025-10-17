using ETLProject.Core.Models;
using ETLProject.Processor;

namespace ETLProject.Tests;

public class TaxiTripProccessorTests
{
    [Fact]
    public async Task ProcessAsync_ShouldDetectDuplicates_AndNormalizeFlags()
    {
        // Arrange
        var processor = new TaxiTripProcessor();
        var baseTime = new DateTime(2024, 1, 1, 10, 0, 0);

        async IAsyncEnumerable<TaxiTrip> GetTrips()
        {
            yield return new TaxiTrip
            {
                TpepPickupDatetime = baseTime,
                TpepDropoffDatetime = baseTime.AddMinutes(10),
                PassengerCount = 1,
                StoreAndFwdFlag = "Y"
            };
            yield return new TaxiTrip
            {
                TpepPickupDatetime = baseTime,
                TpepDropoffDatetime = baseTime.AddMinutes(10),
                PassengerCount = 1,
                StoreAndFwdFlag = "N"
            };
            yield return new TaxiTrip
            {
                TpepPickupDatetime = baseTime.AddMinutes(20),
                TpepDropoffDatetime = baseTime.AddMinutes(30),
                PassengerCount = 2,
                StoreAndFwdFlag = "n"
            };
            await Task.Yield();
        }

        // Act
        var result = await processor.ProcessAsync(GetTrips());

        // Assert
        Assert.Single(result.DuplicateTrips);
        Assert.Equal(2, result.ValidTrips.Count);
        Assert.All(result.ValidTrips, t =>
            Assert.True(t.StoreAndFwdFlag is "Yes" or "No" or "N/A"));
    }


    private async IAsyncEnumerable<Core.Models.TaxiTrip> GetTrips()
    {
        yield return new Core.Models.TaxiTrip { TpepPickupDatetime = DateTime.Now, TpepDropoffDatetime = DateTime.Now.AddMinutes(10), PassengerCount = 1, StoreAndFwdFlag = "Y" };
        yield return new Core.Models.TaxiTrip { TpepPickupDatetime = DateTime.Now, TpepDropoffDatetime = DateTime.Now.AddMinutes(10), PassengerCount = 1, StoreAndFwdFlag = "N" };
        yield return new Core.Models.TaxiTrip { TpepPickupDatetime = DateTime.Now.AddMinutes(20), TpepDropoffDatetime = DateTime.Now.AddMinutes(30), PassengerCount = 2, StoreAndFwdFlag = "n" };
        await Task.Yield();
    }

    [Fact]
    public async Task ProcessAsync_ShouldConvertTimesToUtc()
    {
        // Arrange
        var processor = new TaxiTripProcessor();
        var now = new DateTime(2024, 1, 1, 12, 0, 0);

        async IAsyncEnumerable<TaxiTrip> GetTrips()
        {
            yield return new TaxiTrip
            {
                TpepPickupDatetime = now,
                TpepDropoffDatetime = now.AddMinutes(5),
                PassengerCount = 1
            };
            await Task.Yield();
        }

        // Act
        var result = await processor.ProcessAsync(GetTrips());

        // Assert
        var trip = result.ValidTrips.First();
        Assert.Equal(DateTimeKind.Utc, trip.TpepPickupDatetime.Kind);
        Assert.True(trip.TpepDropoffDatetime > trip.TpepPickupDatetime);
    }
}
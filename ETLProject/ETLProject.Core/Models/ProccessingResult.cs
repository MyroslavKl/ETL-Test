using ETLProject.Core.Models;

namespace ETLProject.Processor;

//Created Record for saving the result
public record ProcessingResult(List<TaxiTrip> ValidTrips, List<TaxiTrip> DuplicateTrips);

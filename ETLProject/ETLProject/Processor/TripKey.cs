using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETLProject.Processor;

public record TripKey(DateTime PickupTime, DateTime DropoffTime, int? PassengerCount);

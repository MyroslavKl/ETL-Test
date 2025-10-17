using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETLProject.Processor;

//Identified variables according to which uniqueness is determined
public record TripKey(DateTime PickupTime, DateTime DropoffTime, int? PassengerCount);

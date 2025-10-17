CREATE DATABASE TaxiDb;

CREATE TABLE TaxiTripData(
  Id INT PRIMARY KEY IDENTITY(1,1),
  tpep_pickup_datetime DATETIME2,
  tpep_dropoff_datetime DATETIME2,
  passenger_count SMALLINT,
  trip_distance DECIMAL(6,3),
  store_and_fwd_flag VARCHAR(3),
  PULocationID SMALLINT,
  DOLocationID SMALLINT,
  fare_amount DECIMAL(5,2),
  tip_amount DECIMAL(5,2)
  );

  CREATE INDEX IX_TaxiTripData_PULocationID ON TaxiTripData(PULocationID);
  CREATE INDEX IX_TaxiTripData_TripDistance ON TaxiTripData(trip_distance DESC);
  CREATE INDEX IX_TaxiTripData_DropoffDatetime ON TaxiTripData (tpep_dropoff_datetime);

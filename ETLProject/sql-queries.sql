 SELECT TOP 1 
    PULocationID, 
    AVG(tip_amount) AS AverageTip
FROM TaxiTripData
GROUP BY PULocationID
ORDER BY AverageTip DESC;

SELECT TOP 100 *
FROM TaxiTripData
ORDER BY trip_distance DESC;

SELECT TOP 100 *,
       DATEDIFF(MINUTE, tpep_pickup_datetime, tpep_dropoff_datetime) AS MinutesTraveled
FROM TaxiTripData
ORDER BY MinutesTraveled DESC;

SELECT *
FROM TaxiTripData
WHERE PULocationID = 132; --example
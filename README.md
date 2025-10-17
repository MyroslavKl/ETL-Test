Here, I will provide additional information about the test task.

First, after executing the query:  SELECT COUNT(*) FROM TaxiTripData;
I received 29889 records on my table.

Second, about the assumptions about a 10GB CSV file. For now, my app saves the data into the lists: "validlist" and "duplicateslist", so when the input file is 10GB of memory, I will get an error about OutOfMemory.
So, for this, I think I will use a batch size of 50000 records and immediately handle it. So it will handle one by one, not a full size.

Also adding here the logs:
[18:09:06 INF] ETL process starting.
[18:09:06 INF] Extracting data from https://drive.google.com/uc?export=download&id=1l2ARvh1-tJBqzomww45TrGtIh5j8Vud4...
[18:09:06 INF] Transforming data...
[18:09:10 INF] Transformation complete. Found 29889 valid trips and 111 duplicates.
[18:09:10 INF] Starting bulk insert of 29889 trips.
[18:09:11 INF] Successfully inserted 29889 trips in 880 ms.
[18:09:11 INF] Writing 111 duplicates to file: C:\taxitripdata\duplicates.csv
[18:09:11 INF] Successfully wrote duplicates to file.
[18:09:11 INF] ETL process finished successfully. Final row count in table: 29889

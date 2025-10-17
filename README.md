Here, I will provide additional information about the test task.

First, after executing the query:  SELECT COUNT(*) FROM TaxiTripData;
I received 29889 records on my table.

Second, about the assumptions about a 10GB CSV file. For now, my app saves the data into the lists: "validlist" and "duplicateslist", so when the input file is 10GB of memory, I will get an error about OutOfMemory.
So, for this, I think I will use a batch size of 50000 records and immediately handle it. So it will handle one by one, not a full size.

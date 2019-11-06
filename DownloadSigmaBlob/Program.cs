using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Text;

namespace DownloadSigmaBlob
{
    class Program
    {
        private static Stream stream;

        static async Task Main(string[] args)
        {
            await GetHelloWorldAsync();
        }

        private static async Task GetHelloWorldAsync()
        {

            // Retrieve the connection string for use with the application. The storage 
            // connection string is stored in an environment variable on the machine 
            // running the application called CONNECT_STR. If the 
            // environment variable is created after the application is launched in a 
            // console or with Visual Studio, the shell or application needs to be closed
            // and reloaded to take the environment variable into account.
            string storageConnectionString = "BlobEndpoint=https://sigmaiotexercisetest.blob.core.windows.net/;QueueEndpoint=https://sigmaiotexercisetest.queue.core.windows.net/;FileEndpoint=https://sigmaiotexercisetest.file.core.windows.net/;TableEndpoint=https://sigmaiotexercisetest.table.core.windows.net/;SharedAccessSignature=sv=2017-11-09&ss=bfqt&srt=sco&sp=rl&se=2028-09-27T16:27:24Z&st=2018-09-27T08:27:24Z&spr=https&sig=eYVbQneRuiGn103jUuZvNa6RleEeoCFx1IftVin6wuA%3D";

            // Check whether the connection string can be parsed.
            CloudStorageAccount storageAccount;
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                // Create the CloudBlobClient that represents the 
                // Blob storage endpoint for the storage account.
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                var result = cloudBlobClient.Credentials;
                BlobContinuationToken continuationToken = null;

                var cloudBlobContainer = cloudBlobClient.GetContainerReference("iotbackend");
                await ListBlobsFlatListingAsync(cloudBlobContainer, null);
            }
            else
            {
                // Otherwise, let the user know that they need to define the environment variable.
                Console.WriteLine(
                    "A connection string has not been defined in the system environment variables. " +
                    "Add an environment variable named 'CONNECT_STR' with your storage " +
                    "connection string as a value.");
                Console.WriteLine("Press any key to exit the application.");
                Console.ReadLine();
            }
        }

        private static async Task ListBlobsFlatListingAsync(CloudBlobContainer container, int? segmentSize)
        {
            BlobContinuationToken continuationToken = null;
            CloudBlob blob;

            try
            {
                // Call the listing operation and enumerate the result segment.
                // When the continuation token is null, the last segment has been returned
                // and execution can exit the loop.
                do
                {
                    BlobResultSegment resultSegment = await container.ListBlobsSegmentedAsync(string.Empty, true, BlobListingDetails.Metadata, segmentSize, continuationToken, null, null);

                    List<SensorData> sensorData = new List<SensorData>();
                    foreach (var blobItem in resultSegment.Results)
                    {
                        // A flat listing operation returns only blobs, not virtual directories.
                        blob = (CloudBlob)blobItem;

                        using (var mStream = new MemoryStream())
                        {
                            if (!blob.Name.Contains("historical") && !blob.Name.Contains("metadata"))
                            {
                                SensorData item = null;
                                if (blob.Name.Contains("humidity"))
                                {
                                    item = new Humidity();
  
                                } 
                                else if (blob.Name.Contains("rainfall"))
                                {
                                    item = new Rainfall();
                                } 
                                else if (blob.Name.Contains("temperature"))
                                {
                                    item = new Temperature();
                                }

                                await blob.DownloadToStreamAsync(mStream);
                                var result = mStream.ToArray();
                                string converted = Encoding.UTF8.GetString(result, 0, result.Length);

                                item.MeasurementData = new List<double>();
                                item.MeasurementDay = DateTime.Now;
                                item.MeasurementTime = new List<DateTime>();
                                sensorData.Add(item);
                            }
                        }

                        // Write out some blob properties.
                        Console.WriteLine("Blob name: {0}", blob.Name);
                    }

                    foreach (var item in sensorData)
                    {
                        var result = item.GetType();
                    }

                        Console.WriteLine();

                    // Get the continuation token and loop until it is null.
                    continuationToken = resultSegment.ContinuationToken;

                } while (continuationToken != null);
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }
    }
}

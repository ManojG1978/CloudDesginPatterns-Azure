using System;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;

namespace ValetKey
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var bobClient = await GreatBlobContainer();

            var uri = GetServiceSasUriForBlob(bobClient);

            await ReadBlobWithSasAsync(uri);

            Console.WriteLine("Done!");
            
            Console.ReadLine();
        }
        
        static async Task ReadBlobWithSasAsync(Uri sasUri)
        {
            // Try performing a read operation using the blob SAS provided.

            // Create a blob client object for blob operations.
            BlobClient blobClient = new BlobClient(sasUri, null);

            // Download and read the contents of the blob.
            try
            {
                Console.WriteLine("Blob contents:");

                // Download blob contents to a stream and read the stream.
                BlobDownloadInfo blobDownloadInfo = await blobClient.DownloadAsync();
                using (StreamReader reader = new StreamReader(blobDownloadInfo.Content, true))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Console.WriteLine(line);
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Read operation succeeded for SAS {0}", sasUri);
                Console.WriteLine();
            }
            catch (RequestFailedException e)
            {
                // Check for a 403 (Forbidden) error. If the SAS is invalid, 
                // Azure Storage returns this error.
                if (e.Status == 403)
                {
                    Console.WriteLine("Read operation failed for SAS {0}", sasUri);
                    Console.WriteLine("Additional error information: " + e.Message);
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine(e.Message);
                    Console.ReadLine();
                    throw;
                }
            }
        }

        private static async Task<BlobClient> GreatBlobContainer()
        {
            string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");

            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            //Create a unique name for the container
            string containerName = "valetblob" + Guid.NewGuid();

            // Create the container and return a container client object
            BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);


            // Create a local file in the ./data/ directory for uploading and downloading
            string localPath = "data";
            string fileName = $"valetsample-{Guid.NewGuid()}.txt";
            string localFilePath = Path.Combine(Environment.CurrentDirectory, localPath);
            if(!Directory.Exists(localFilePath))
                Directory.CreateDirectory(localFilePath);

            localFilePath = Path.Combine(localFilePath, fileName);
            // Write text to the file
            await File.WriteAllTextAsync(localFilePath, $"Hello, World! {DateTime.Now}");

            // Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

            // Open the file and upload its data
            using FileStream uploadFileStream = File.OpenRead(localFilePath);
            await blobClient.UploadAsync(uploadFileStream, true);
            return blobClient;
        }

        private static Uri GetServiceSasUriForBlob(BlobClient blobClient,
            string storedPolicyName = null)
        {
            // Check whether this BlobClient object has been authorized with Shared Key.
            if (blobClient.CanGenerateSasUri)
            {
                // Create a SAS token that's valid for one hour.
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                    BlobName = blobClient.Name,
                    Resource = "b"
                };

                if (storedPolicyName == null)
                {
                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
                    sasBuilder.SetPermissions(BlobSasPermissions.Read |
                                              BlobSasPermissions.Write);
                }
                else
                {
                    sasBuilder.Identifier = storedPolicyName;
                }

                Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
                Console.WriteLine("SAS URI for blob is: {0}", sasUri);
                Console.WriteLine();

                return sasUri;
            }
            else
            {
                Console.WriteLine(@"BlobClient must be authorized with Shared Key 
                          credentials to create a service SAS.");
                return null;
            }
        }
        
    }
}

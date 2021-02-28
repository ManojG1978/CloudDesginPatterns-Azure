using Bogus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace CovidSample
{
    public static class SoundFileProcessor
    {
        [FunctionName("SoundFileProcessor")]
        [return: Queue("transcribed-voice-files")]
        public static string Run([QueueTrigger("covid-voice-files", Connection = "storageConnectionString")]string inputQueueItem, ILogger log)
        {
            dynamic jsonRequest = JObject.Parse(inputQueueItem);
            var faker = new Faker();
            
            //Enrich existing message with a translation (bogus message)
            jsonRequest["translation"] = faker.Lorem.Paragraph();
            
            log.LogInformation($"C# Queue trigger function processed: {inputQueueItem}");

            return jsonRequest.ToString();
            
            
        }
    }
}

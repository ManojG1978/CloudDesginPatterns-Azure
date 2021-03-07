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
            
            // Enrich existing message with a translation (bogus message). 
            // This is only a simulation. In a real scenario, you could employ a service like Azure Cognitive Service - Speech to Text for this
            jsonRequest["translation"] = faker.Lorem.Paragraph();
            
            log.LogInformation($"SoundFileProcessor function processed: {inputQueueItem}");

            return jsonRequest.ToString();
        }
    }
}

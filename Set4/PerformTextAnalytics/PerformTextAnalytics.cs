using Bogus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace PipesAndFilter
{
    public static class PerformTextAnalytics
    {
        [FunctionName("PerformTextAnalytics")]
        [return: Queue("processed-voice-files")]
        public static string Run(
            [QueueTrigger("transcribed-voice-files", Connection = "storageConnectionString")]string inputQueueItem, 
            ILogger log)
        {
            // This is only a simulation. In a real scenario, you could employ a service like Azure Cognitive Service - Text Analytics for this
            dynamic jsonRequest = JObject.Parse(inputQueueItem);
            var faker = new Faker();
            
            jsonRequest.KeyPhrases = new JArray() as dynamic;
            jsonRequest.KeyPhrases.Add(faker.Random.Words());
            jsonRequest.KeyPhrases.Add(faker.Random.Words());
            jsonRequest.KeyPhrases.Add(faker.Random.Words());

            log.LogInformation($"PerformTextAnalytics trigger function processed: {inputQueueItem}");

            return jsonRequest.ToString();
        }
    }
}

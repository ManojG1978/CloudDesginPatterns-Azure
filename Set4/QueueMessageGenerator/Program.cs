using System;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Bogus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace QueueMessageGenerator
{
    class Program
    {
        static IConfiguration Configuration { get; set; }
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("Let's queue some messages, how many do you want?");
            var requestedAmount = DetermineMessageCount();
            await QueueMessages(requestedAmount);

            Console.WriteLine("That's it, see you later!");
        }
        
        private static async Task QueueMessages(int requestedAmount)
        {
            ConfigurationBuilder configurationBuilder =  new ConfigurationBuilder();
            Configuration = configurationBuilder.AddJsonFile("appSettings.json").Build();

            QueueClient queueClient = new QueueClient(Configuration["connectionString"], Configuration["queueName"]);
            for (int currentOrderAmount = 0; currentOrderAmount < requestedAmount; currentOrderAmount++)
            {
                var message = GenerateMessage();
                var rawMessage = JsonConvert.SerializeObject(message);
                
                Console.WriteLine($"Queuing message {message.MessageId} - Locale: {message.Language}, URL: {message.FileLocation}");
                await queueClient.SendMessageAsync(Base64Encode(rawMessage));
            }
        }
        
        private static QueueMessage GenerateMessage()
        {
            var queueMessageGenerator = new Faker<QueueMessage>()
                .RuleFor(u => u.FileLocation, (f, u) => f.Internet.Avatar())
                .RuleFor(u => u.MessageId, (f, u) => f.Random.Int())
                .RuleFor(u => u.Language, (f, u) => f.Random.RandomLocale());

            return queueMessageGenerator.Generate();
        }
        
        private static int DetermineMessageCount()
        {
            var rawAmount = Console.ReadLine();
            if (int.TryParse(rawAmount, out int amount))
            {
                return amount;
            }

            Console.WriteLine("That's not a valid number, let's try that again");
            return DetermineMessageCount();
        }
        
        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }


        class QueueMessage
        {
            [JsonPropertyName("messageId")]
            public int MessageId { get; set; }
            
            [JsonPropertyName("fileLocation")]
            public string FileLocation { get; set; }

            [JsonPropertyName("language")]
            public string Language { get; set; }

        }
    }
}

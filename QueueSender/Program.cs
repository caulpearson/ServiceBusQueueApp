using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;


namespace QueueSender
{
    class Program
    {
        // connection string to your Service Bus namespace
        static string connectionString = "";

        // name of my service bus queue
        static string queueName = "caulfield-queue";

        // the client that owns the connection and can be used to create senders and receivers
        static ServiceBusClient client;

        // the sender used to publish messages to the queue
        static ServiceBusSender sender;

        // number of messages to be sent to the queue
        private const int numOfMessages = 3;

        static async Task Main()
        {
            

            var clientOptions = new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets };
            client = new ServiceBusClient(connectionString, clientOptions);
            sender = client.CreateSender(queueName);

            // create a batch 
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            for (int i = 1; i <= numOfMessages; i++)
            {
                // trying to add batch message
                if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i}")))
                {
                    // too large?
                    throw new Exception($"The message {i} is too large to fit in the batch.");
                }
            }

            try
            {
                // Sending batch of messages to the Service Bus queue with producer client
                await sender.SendMessagesAsync(messageBatch);
                Console.WriteLine($"A batch of {numOfMessages} messages has been published to the queue.");
            }
            finally
            {
                // cleaning up network resources and unmanaged objects
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }

            Console.WriteLine("Press any key to end the application");
            Console.ReadKey();
        }
    }
}
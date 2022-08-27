using Azure.Messaging.ServiceBus;
using System.Text;

namespace SessionQueueSender
{
    class Program
    {
        // connection string to your Service Bus namespace
        static string connectionString = "Endpoint=sb://cauls-servicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=a3q8JWEpTe6HnyAtsPKv/I6rslup0c7NcVGQrwURe9I=";

        // name of my service bus queue
        static string queueName = "caulfield-session-queue";

        // the client that owns the connection and can be used to create senders and receivers
        static ServiceBusClient client;

        // the sender used to publish messages to the queue
        static ServiceBusSender sender;

        // number of messages to be sent to the queue
        private const int numOfMessages = 4;

        static async Task Main()
        {
            var clientOptions = new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets };
            client = new ServiceBusClient(connectionString, clientOptions);
            sender = client.CreateSender(queueName);

            //Creating a batch of session messages
            // create a batch 
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
            
            int numOfSessions = 2;
            for(int s = 1; s <= numOfSessions; s++)
            {
                for (int i = 1; i <= numOfMessages; i++)
                {
                    Console.WriteLine($"Adding  message {i+numOfMessages*(s-1)} with Session ID {s} to batch");
                    // trying to add batch messages for session 1
                    if (!messageBatch.TryAddMessage(
                        new ServiceBusMessage($"Message {i+numOfMessages*(s-1)}") { SessionId = s.ToString() }))
                    {
                        // too large?
                        throw new Exception($"The session {s} message {i} is too large to fit in the batch.");
                    }
                }
            }

            try
            {
                // Sending batch of messages to the Service Bus queue with producer client
                await sender.SendMessagesAsync(messageBatch);
                Console.WriteLine($"A batch of {numOfMessages*numOfSessions} messages has been published to the queue.");
            }
            finally
            {
                // cleaning up network resources and unmanaged objects
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }


        }
    }
}
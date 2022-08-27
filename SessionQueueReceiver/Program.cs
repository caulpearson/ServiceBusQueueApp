using Azure.Messaging.ServiceBus;

namespace SessionQueueReceiver
{
    class Program
    {
        static string connectionString = "Endpoint=sb://cauls-servicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=a3q8JWEpTe6HnyAtsPKv/I6rslup0c7NcVGQrwURe9I=";
        static string queueName = "caulfield-session-queue";
        static ServiceBusClient client;
        //session processor that reads and processes messages from the queue
        static ServiceBusSessionProcessor sessionProcessor;
        
        static async Task MessageHandler(ProcessSessionMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();

            Console.WriteLine($"Received: {body}");

            await args.CompleteMessageAsync(args.Message);
        }

        static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        static async Task Main()
        {
            var clientOptions = new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets };
            client = new ServiceBusClient(connectionString, clientOptions);

            
            int sessions = 2;
            try
            {
                for(int i = 1; i <= sessions; i++)
                {
                    // create a processor that we can use to process the messages
                    var options = new ServiceBusSessionProcessorOptions
                    {
                        SessionIds = { i.ToString() }
                    };
                    sessionProcessor = client.CreateSessionProcessor(queueName, options);

                    sessionProcessor.ProcessMessageAsync += MessageHandler;

                    sessionProcessor.ProcessErrorAsync += ErrorHandler;

                    await sessionProcessor.StartProcessingAsync();
                }
                

                Console.ReadKey();

                Console.WriteLine("\nStopping receiver...");
                await sessionProcessor.StopProcessingAsync();
                Console.WriteLine("No longer receiving messages");
            }
            finally
            {
                // Cleaning up network resources and unmanaged objects
                await sessionProcessor.DisposeAsync();
                await client.DisposeAsync();
            }
        }
    }
}
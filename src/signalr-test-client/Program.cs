using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace signalr_test_client
{
    class Program
    {
        static HubConnection connection;

        public static async Task<int> Main(string[] args)
        {
           await t();
           return 0;
        }

        static async Task t(){
            connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5051/eventhub")
                .Build();
            connection.On<string, string>("publishmessage", (topic, message) =>
            {
                Console.WriteLine($"{topic}: {message}");
            });
            connection.On<string>("echo", (echo) =>
            {
                Console.WriteLine($"echo: {echo}");
            });
  
            connection.On<string>("subscribe", (echo) =>
            {
                Console.WriteLine($"subscribed: {echo}");
            });

            await connection.StartAsync();

            Console.WriteLine("started...");

            var x = new Random().Next();
            while (true)
            {

                await connection.InvokeAsync("Subscribe", "topic");
                await connection.InvokeAsync("PublishMessage","topic", "Hello World! " + x);
                await connection.InvokeAsync("Echo","Hello World! " + x);
                Task.Delay(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
                x++;
            }
        }
    }
}

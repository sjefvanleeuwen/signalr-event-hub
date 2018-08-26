using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;

namespace signalr_event_hub.Hubs
{
    public class EventHub : Hub
    {
        public async Task Subscribe(string topic)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, topic);
            await Clients.Caller.SendAsync("subscribe",topic);
            Console.WriteLine("subscribed..");
        }
        public void Unsubscribe(string topic)
        {
            Groups.RemoveFromGroupAsync(Context.ConnectionId, topic);
            System.Diagnostics.Trace.WriteLine("unsubscribed..");
        }

        public void Echo(string echo)
        {
            Console.WriteLine($"echo: {echo}");
            Clients.All.SendAsync("echo",echo);
        }

        public async Task PublishMessage(string topic, string message)
        {
            await Clients.Group(topic).SendAsync("publishmessage",topic,message);
          //  await Clients.Caller.SendAsync("publishmessage",topic,message);
            Console.WriteLine($"{topic}:{message}");
        }
    }
}
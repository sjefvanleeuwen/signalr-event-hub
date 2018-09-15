using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CamundaClient.Dto;
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

        public async Task PublishMessage(string topic, string message, string data,string processdata)
        {
            var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string,object>>(data);
            if (topic.ToLower() == "startprocess") {
                 Program.camunda.BpmnWorkflowService.StartProcessInstance(message, dict);
                 return;
            }

            if (topic == "humanTask") {
                ExternalTask t = Newtonsoft.Json.JsonConvert.DeserializeObject<ExternalTask>(processdata);
                //Console.WriteLine("HUMANTASK" + "Task_0b1rq1j:" + t.ProcessInstanceId);
                var q = new Dictionary<string,string>();
                q.Add("id",t.ProcessInstanceId);
                var task = Program.camunda.HumanTaskService.LoadTasks(q).First();
                Console.WriteLine("HUMANTASK " + task.Id);
                var values = new Dictionary<string,object>();
                values.Add("isManual",true);
                Program.camunda.HumanTaskService.Complete(task.Id,values);
                return;
            }
            //var data = string.Empty;
            if (message.StartsWith("redis_get!"))
                data = Program.Db.StringGet(message);
            await Clients.Group(topic).SendAsync("publishmessage",topic,message,data, processdata);
            //await Clients.Caller.SendAsync("publishmessage",topic,message);
            Console.WriteLine($"{topic}:{message}:{processdata}");
        }
    }
}
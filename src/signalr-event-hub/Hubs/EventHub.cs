using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CamundaClient.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;

namespace signalr_event_hub.Hubs
{
    [Authorize]
    public class EventHub : Hub
    {
        [Authorize]
        public async Task StringSet(string key, string value){
            await Program.Db.StringSetAsync(key,value,new TimeSpan(6,0,0,0));
        }
            [Authorize]

         public async Task<string> StringGet(string key){
            return Program.Db.StringGet(key);
        }

        public async Task Subscribe(string topic)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, topic);
            Console.WriteLine("subscribed.." + topic);
        }

        public async Task Connected(){
            await Clients.All.SendAsync("connected",Context.ConnectionId);
        }

        public void Unsubscribe(string topic)
        {
            Groups.RemoveFromGroupAsync(Context.ConnectionId, topic);
            System.Diagnostics.Trace.WriteLine("unsubscribed..");
        }

        [Authorize]
        public void Echo(string echo)
        {
            Console.WriteLine($"{Context.User.Claims.ElementAt(1).Value}  echo: {echo}");
            Clients.All.SendAsync("echo",echo);
        }

        public async Task<List<ProcessDefinition>> GetProcessDefinitions()
        {
            return Program.camunda.RepositoryService.LoadProcessDefinitions(true);
        }

        public async Task<string> StartProcessInstance(string processDefinitionKey, string businessKey, string variables) {
            businessKey = new Random().Next().ToString();
            Console.WriteLine("Start Process Instance");
            Console.WriteLine("----------------------");
            var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string,object>>(variables);
            Console.WriteLine($"(ProcessDefinitionKey): {processDefinitionKey} (BusinessKey): {businessKey} (Variables): {variables}");
            Console.WriteLine("----------------------");
            return Program.camunda.BpmnWorkflowService.StartProcessInstance(processDefinitionKey,businessKey, dict);
        }

        public async Task CompleteTask(string taskId, string values) {
            //ExternalTask t = Newtonsoft.Json.JsonConvert.DeserializeObject<ExternalTask>(processdata);
            Console.WriteLine($"Complete Task: {taskId}");
            var q = new Dictionary<string,string>();
            q.Add("Id",taskId);
            var task = Program.camunda.HumanTaskService.LoadTasks(q).First();
            var v = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string,object>>(values);
            Program.camunda.HumanTaskService.Complete(task.Id,v);
            return;
        }

        public async Task PublishMessage(string topic, string message, string data,string processdata)
        {
            await Clients.Group(topic).SendAsync("publishmessage",topic,message,data, processdata);
            Console.WriteLine($"(topic): {topic} (message):{message} (processData):{processdata}");
            Console.WriteLine($"(data): {data}");
        }

    }
}
﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Camunda.Api.Client;
using Camunda.Api.Client.ExternalTask;
using Camunda.Api.Client.UserTask;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace signalr_test_client
{
    public class User
    {
        public string ConnectionId {get;set;}
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
    }

    class Program
    {
        static HubConnection connection;
        static HubConnection auth;
        public static CamundaClient camunda = CamundaClient.Create("http://localhost:8080/engine-rest");

        public static async Task<int> Main(string[] args)
        {
           await t();
           return 0;
        }

        static async Task t(){
            auth = new HubConnectionBuilder()
                .WithUrl("http://localhost:5051/authenticationhub")
                .Build();



            // connection.On<string, string>("publishmessage", (topic, message) =>
            // {
            //     Console.WriteLine($"{topic}: {message}");
            // });
            // connection.On<string>("echo", (echo) =>
            // {
            //     Console.WriteLine($"echo: {echo}");
            // });
  
            // connection.On<string>("subscribe", (echo) =>
            // {
            //     Console.WriteLine($"subscribed: {echo}");
            // });
            await auth.StartAsync();
            var s = await auth.InvokeAsync<User>("Authenticate","serviceaccount","test");

            connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5051/eventhub",options => {
                     options.AccessTokenProvider = () => Task.FromResult(s.Token);
                })
                .Build();


            await connection.StartAsync();


            await connection.InvokeAsync("echo","hi");
            Console.ReadLine();

            // await connection.InvokeAsync("subscribe","topic2");
            var x = new Random().Next();
            var lastQuery = DateTime.MinValue;
            var total=0;
            DateTime lastQueryExecution = DateTime.MinValue;
            while (true)
            {
                if (lastQuery != DateTime.MinValue)
                    lastQueryExecution = DateTime.Now;
                var externalTaskQuery = new TaskQuery() { Active = true,  /* broken: CreatedAfter = DateTime.Now */};
                // add some sorting
                externalTaskQuery.Sort(TaskSorting.Created, SortOrder.Descending);
                // request external tasks according to query
                List<UserTaskInfo> tasks = await camunda.UserTasks.Query(externalTaskQuery).List();
                var q = tasks.Where(p=>p.Created>=lastQuery);
                total+=q.Count();
                if (q.Count()>0){
                    var i=0;
                    Console.WriteLine($"publish message {q.Count()} new messages out of {total}.");
                    // only do sparse queries for now.
                    if (q.Count()<10){
                        foreach (var task in q) {
                            i++;
                            Console.WriteLine($"pushing task id ${task} process data {i} out of {q.Count()}.");
                            connection.InvokeAsync("PublishMessage","human-task-data","info","",JsonConvert.SerializeObject(task));
                        }
                        Console.WriteLine("showing the first task for debug purposes");
                        Console.WriteLine("-----------------------------------------");
                        Console.WriteLine(JsonConvert.SerializeObject(q.First()));
                        Console.WriteLine("-----------------------------------------");
                    }
                    await connection.InvokeAsync("PublishMessage","dashboard-human-tasks",q.Count(),q.Count(),"");
                }
                lastQuery=DateTime.Now;
                Task.Delay(TimeSpan.FromSeconds(20)).GetAwaiter().GetResult();
                x++;
            }
        }
    }
}

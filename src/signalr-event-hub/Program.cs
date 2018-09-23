using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics.AspNetCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CamundaClient;
using StackExchange.Redis;

namespace signalr_event_hub
{
    public class Program
    {
        private  static  ConnectionMultiplexer Redis;
        public static CamundaEngineClient camunda = new CamundaEngineClient();     
        public static IDatabase Db;

        public static void Main(string[] args)
        {
            Redis = ConnectionMultiplexer.Connect("localhost");
            Db = Redis.GetDatabase();
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseMetrics()
                .UseStartup<Startup>()
                .UseUrls("http://*:5051")
                .Build();
    }
}

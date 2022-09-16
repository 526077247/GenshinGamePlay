using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ETFileServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var configuration = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory)
                                                            .AddJsonFile("hosting.json")
                                                            .Build();
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder

                    .UseKestrel()
                    .UseConfiguration(configuration)
                    .UseUrls(configuration["server.urls"])
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseStartup<Startup>();
                });
        }
    }
}

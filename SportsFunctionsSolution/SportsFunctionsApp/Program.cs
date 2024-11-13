using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
//using Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SportsFunctionsApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWebApplication()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .Build();

            await host.RunAsync();
        }
    }
}

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SportsFunctionsApp.Utilities;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace SportsFunctionsApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var azureCredentialOptions = new DefaultAzureCredentialOptions();
            var credential = new DefaultAzureCredential(azureCredentialOptions);
            var AzKeyVaultUri = Environment.GetEnvironmentVariable("AzKeyVaultUri");

            // Create a SecretClient
            var secretClient = new SecretClient(new Uri(AzKeyVaultUri.ToString()), credential);
            var Configuration = new ConfigurationBuilder()
                    .AddAzureKeyVault(new Uri(AzKeyVaultUri),
                        new DefaultAzureCredential())
                    .Build();
            string SQLconnStr = Configuration["WTT-SQLdbConnStr"];
            string cosmosDBpk = Configuration["SanWTTCosmosDB-PrimaryKey"];

            var host = new HostBuilder()
                .ConfigureFunctionsWebApplication()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureServices(services => services.AddSingleton<CosmosDBSetup>())
                .ConfigureServices(services => services.AddSingleton(sqldb => new SqlDbHelper(SQLconnStr)))
                .ConfigureServices(services => services.AddSingleton(sp => new CosmosDbHelper(
                    Environment.GetEnvironmentVariable("CosmosDB_Endpoint"), cosmosDBpk, "WTT_SportsDB")))
                .Build();

            if (Environment.GetEnvironmentVariable("Database") == "Cosmos")
            {
                CosmosDBSetup cosmosDBSetup = new CosmosDBSetup();
                await cosmosDBSetup.CosmosDBsetup();
            }
            await host.RunAsync();
        }
    }
}

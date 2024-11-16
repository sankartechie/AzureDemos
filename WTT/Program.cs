using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
//using Microsoft.Azure.Functions.Worker.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.Extensions.Configuration;
//using Microsoft.AspNetCore.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);
//var azureCredentialOptions = new DefaultAzureCredentialOptions();
//var credential = new DefaultAzureCredential(azureCredentialOptions);
//var AzKeyVaultUri = Environment.GetEnvironmentVariable("AzKeyVaultUri");

// Create a SecretClient
//var secretClient = new SecretClient(new Uri(AzKeyVaultUri), credential);

builder.ConfigureFunctionsWebApplication();
//builder.Configuration.AddAzureKeyVault(
//    new Uri(AzKeyVaultUri),
//    new DefaultAzureCredential()
//);
//builder.Services.AddAzureFunctionsWorker(); // Azure Functions Worker support

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
//builder.Services
//    .AddApplicationInsightsTelemetryWorkerService()
//    .ConfigureFunctionsApplicationInsights();

var app = builder.Build();
app.Run();

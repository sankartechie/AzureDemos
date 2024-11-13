using AzureFunctionsWebApi.Models;
using AzureFunctionsWebApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Register HttpClient and AzureFunctionsClient
builder.Services.AddHttpClient<IAzureFunctionsClient, AzureFunctionsClient>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMvcCore();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WTT API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseDefaultFiles();
app.UseHttpsRedirection();
app.UseAuthorization();

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.UseEndpoints(e => { });

app.UseStatusCodePagesWithRedirects("/errors/{0}");
app.MapGet("/errors/404", () => "Not found page");

app.Run();

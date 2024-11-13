
using AzureFunctionsWebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Register HttpClient and AzureFunctionsClient
builder.Services.AddHttpClient<IAzureFunctionsClient, AzureFunctionsClient>();
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.MapControllers();
app.MapGet("/", () => "Hello World!");
app.MapGet("/Matches", () => "This is an example!");
app.MapGet("/Players", () => "Players!");
app.UseStatusCodePagesWithRedirects("/errors/{0}");
app.MapGet("/errors/404", () => "Not found page");

app.Run();

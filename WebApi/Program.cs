using Microsoft.AspNetCore.Mvc.Formatters; // IOutputFormatter, OutputFormatter
using West.Shared; // AddNorthwindContext extension method
using WebApi.Repositories; // ICustomerRepository, CustomerRepository
using Swashbuckle.AspNetCore.SwaggerUI; // SubmitMethod
using Microsoft.AspNetCore.HttpLogging;  // HttpLoggingFields

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddNothwindContext();
builder.Services.AddControllers(options =>
{
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine("Default output formatters:");
    foreach (IOutputFormatter formatter in options.OutputFormatters)
    {
        OutputFormatter? mediaFormatter = formatter as OutputFormatter;
        if (mediaFormatter is null)
        {
            Console.WriteLine($"- {formatter.GetType().Name}");
        }
        else // OutputFormatter class has SupportedMediaTypes
        {
            Console.WriteLine("- {0}, Media types: {1}",
                mediaFormatter.GetType().Name,
                string.Join(", ", mediaFormatter.SupportedMediaTypes));
        }
    }
    Console.ResetColor();
})
    .AddXmlDataContractSerializerFormatters()
    .AddXmlSerializerFormatters();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.All;
    options.RequestBodyLogLimit = 4096;
    options.ResponseBodyLogLimit = 4096;
});
builder.Services.AddW3CLogging(options =>
{
    options.AdditionalRequestHeaders.Add("x-forwarded-for");
    options.AdditionalRequestHeaders.Add("x-client-ssl-protocol");
});
builder.Services.AddHealthChecks()
    .AddDbContextCheck<NorthwindContext>()
    // execute SELECT 1 using the specified connection string
    .AddSqlServer(NorthwindContext.SqlServerConnectionString);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Northwind Service API Version 1");
        options.SupportedSubmitMethods(SubmitMethod.Get, SubmitMethod.Post, SubmitMethod.Put, SubmitMethod.Delete);
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseHealthChecks(path:"/howdoyoufeel");

app.MapControllers();

app.Run();
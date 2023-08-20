using Microsoft.AspNetCore.Mvc.Formatters; // IOutputFormatter, OutputFormatter
using West.Shared; // AddNorthwindContext extension method
using WebApi.Repositories; // ICustomerRepository, CustomerRepository

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddNothwindContext();
builder.Services.AddControllers(options =>
{
    Console.WriteLine("Default output formatters:");
    foreach (IOutputFormatter formatter in options.OutputFormatters)
    {
        OutputFormatter? mediaFormatter = formatter as OutputFormatter;
        if (mediaFormatter is null)
        {
            Console.WriteLine($"    {formatter.GetType().Name}");
        }
        else // OutputFormatter class has SupportedMediaTypes
        {
            Console.WriteLine("    {0}, Media types: {1}",
                mediaFormatter.GetType().Name,
                string.Join(", ", mediaFormatter.SupportedMediaTypes));
        }
    }
})
    .AddXmlDataContractSerializerFormatters()
    .AddXmlSerializerFormatters();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
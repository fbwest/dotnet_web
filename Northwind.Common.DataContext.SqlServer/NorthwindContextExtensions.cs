using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace West.Shared;

public static class NorthwindContextExtensions
{
    /// <summary>
    /// Adds NorthwindContext to the specified IServiceCollection. Uses the SqlServer database provider.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionString">Set to override the default.</param>
    /// <returns>An IServiceCollection that can be used to add more services.</returns>
    public static IServiceCollection AddNothwindContext(this IServiceCollection services,
        string connectionString = "Data Source=.;" +
                                  "Initial Catalog=Northwind;" +
                                  "Integrated Security=true;" +
                                  "MultipleActiveResultsets=true;" +
                                  "Encrypt=false")
    {
        services.AddDbContext<NorthwindContext>(options =>
        {
            options.UseSqlServer(connectionString);
            options.LogTo(Console.WriteLine,
                new[] { Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuting });
        });
        
        return services;
    }
}
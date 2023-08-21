using Microsoft.EntityFrameworkCore.ChangeTracking; // EntityEntry<T>
using West.Shared; // Customer
using System.Collections.Concurrent; // ConcurrentDictionary

namespace WebApi.Repositories;

public class CustomerRepository : ICustomerRepository
{
    // Use a static thread-safe dictionary field to cache the customers.
    private static ConcurrentDictionary<string, Customer>? customersCache;
    
    // Use an instance data context field because it should not be
    // cached due to the data context having internal caching.
    private NorthwindContext db;

    public CustomerRepository(NorthwindContext injectedContext)
    {
        db = injectedContext;
        
        // Pre-load customers from database as a normal
        // Dictionary with CustomerId as the key,
        // then convert to a thread-safe ConcurrentDictionary.
        if (customersCache is null)
        {
            customersCache = new ConcurrentDictionary<string, Customer>(
                db.Customers.ToDictionary(c => c.CustomerId));
        }
    }

    private static Customer UpdateCache(string id, Customer customer)
    {
        Customer? old;
        if (customersCache is not null)
            if (customersCache.TryGetValue(id, out old))
                if (customersCache.TryUpdate(id, customer, old))
                    return customer;
        
        return null!;
    }

    public async Task<Customer?> CreateAsync(Customer customer)
    {
        // Normalize CustomerId into uppercase.
        customer.CustomerId = customer.CustomerId.ToUpper();
        // Add to database using EF Core.
        await db.Customers.AddAsync(customer);

        if (await db.SaveChangesAsync() == 1)
        {
            if (customersCache is null) return customer;
            // If the customer is new, add it to cache, else
            // call UpdateCache method.
            return customersCache.AddOrUpdate(customer.CustomerId, customer, UpdateCache);
        }

        return null;
    }

    public Task<IEnumerable<Customer>> RetrieveAllAsync()
    {
        // For performance, get from cache.
        return Task.FromResult(customersCache?.Values ?? Enumerable.Empty<Customer>());
    }

    public Task<Customer?> RetrieveAsync(string id)
    {
        id = id.ToUpper();
        // For performance, get from cache.
        if (customersCache is null) return null!;

        customersCache.TryGetValue(id, out Customer? customer);
        return Task.FromResult(customer);
    }
    
    public async Task<Customer?> UpdateAsync(string id, Customer customer)
    {
        // Normalize id
        id = id.ToUpper();
        customer.CustomerId = customer.CustomerId.ToUpper();
        // Update database
        db.Customers.Update(customer);
        // update cache
        if (await db.SaveChangesAsync() == 1)
        {
           return UpdateCache(id, customer);
        }
        
        return null;
    }

    public async Task<bool?> DeleteAsync(string id)
    {
        id = id.ToUpper();
        // delete from db
        var customer = await db.Customers.FindAsync(id);
        if (customer is null) return null;
        db.Customers.Remove(customer);
        return await db.SaveChangesAsync() != 1 ? null :
            // delete from cache
            customersCache?.TryRemove(id, out customer);
    }
}
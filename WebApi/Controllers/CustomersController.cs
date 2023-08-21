using Microsoft.AspNetCore.Mvc; // [Route], [ApiController], ControllerBase
using West.Shared; // Customer
using WebApi.Repositories; // ICustomerRepository

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomersController : Controller
{
    private readonly ICustomerRepository _repo;

    // constructor injects repository registered in Startup
    public CustomersController(ICustomerRepository repo)
    {
        _repo = repo;
    }
    
    // GET: api/customers
    // GET: api/customers/?country=[country]
    // this will always return a list of customers (but it might be empty)
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<Customer>))]
    public async Task<IEnumerable<Customer>> GetCustomers(string? country)
    {
        if (string.IsNullOrWhiteSpace(country)) return await _repo.RetrieveAllAsync();
        return (await _repo.RetrieveAllAsync()).Where(c => c.Country == country);
    }
    
    // GET: api/customers/[id]
    [HttpGet("{id}", Name = nameof(GetCustomer))]
    [ProducesResponseType(200, Type = typeof(Customer))]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetCustomer(string id)
    {
        var customer = await _repo.RetrieveAsync(id);
        if (customer is null) return NotFound(); // 404 Resource not found
        return Ok(customer); // 200 OK with customer in body
    }
    
    // POST: api/customers
    // BODY: Customer (JSON, XML)
    [HttpPost]
    [ProducesResponseType(201, Type = typeof(Customer))]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] Customer? customer)
    {
        if (customer is null) return BadRequest();
        if (await _repo.RetrieveAsync(customer.CustomerId) is not null)
            return BadRequest("Customer already exists!");

        if (await _repo.CreateAsync(customer) is null) return BadRequest("Cannot create new customer!");
        return CreatedAtRoute(
            routeName: nameof(GetCustomer),
            routeValues: new { id = customer.CustomerId.ToLower() },
            value: customer);
    }
    
    // PUT: api/customers/[id]
    // BODY: Customer (JSON, XML)
    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(string id, [FromBody] Customer? customer)
    {
        if (customer is null) return BadRequest();  // 400 Bad request
        id = id.ToUpper();
        customer.CustomerId = customer.CustomerId.ToUpper();
        if (id != customer.CustomerId) return BadRequest();  // 400 Bad request
        if (await _repo.RetrieveAsync(id) is null) return NotFound();  // 404 Resource not found
        await _repo.UpdateAsync(id, customer);
        return new NoContentResult(); // 204 No content
    }
    
    // DELETE: api/customers/[id]
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(string id)
    {
        // take control of problem details
        if (id == "bad")
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "https://localhost:5001/customers/failed-to-delete",
                Title = $"Customer {id} found but failed to delete",
                Detail = "More details like Company Name, Country and so on.",
                Instance = HttpContext.Request.Path
            };
            return BadRequest(problemDetails);
        }
        
        if (await _repo.RetrieveAsync(id) == null) return NotFound(); // 404
        var isDeleted = await _repo.DeleteAsync(id);
        if (isDeleted.HasValue && isDeleted.Value) return new NoContentResult(); // 204
        return BadRequest($"Customer {id} was found but failed to delete");
    }
}
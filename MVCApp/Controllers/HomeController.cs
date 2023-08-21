using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MVCApp.Models;
using West.Shared;

namespace MVCApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpClientFactory _clientFactory;

    public HomeController(ILogger<HomeController> logger, IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _clientFactory = clientFactory;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public async Task<IActionResult> Customers(string country)
    {
        string uri;

        if (string.IsNullOrWhiteSpace(country))
        {
            uri = "api/customers";
            ViewData["Title"] = "All customers worlwide";
        }
        else
        {
            uri = $"api/customers/?country={country}";
            ViewData["Title"] = $"Customers in {country}";
        }

        HttpClient client = _clientFactory.CreateClient("WebApi");
        HttpRequestMessage request = new(HttpMethod.Get, uri);
        HttpResponseMessage response = await client.SendAsync(request);
        var model = await response.Content.ReadFromJsonAsync<IEnumerable<Customer>>();

        return View(model);
    }
}
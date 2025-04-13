using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using invmanager.Models;
using Microsoft.Extensions.Logging;

namespace invmanager.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
    
    [Route("Error")]
    public IActionResult Error(int statusCode)
    {
        ViewData["StatusCode"] = statusCode;

        _logger.LogWarning("A {StatusCode} error occurred at {Time}", statusCode, DateTime.Now);

        return View("~/Views/Shared/Error.cshtml");
    }
    

}
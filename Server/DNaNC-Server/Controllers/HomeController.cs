using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DNaNC_Server.Models;
using DNaNC_Server.Services;

namespace DNaNC_Server.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly NodeManagerService _nodeManagerService;

    public HomeController(ILogger<HomeController> logger, NodeManagerService nodeManagerService)
    {
        _logger = logger;
        _nodeManagerService = nodeManagerService;
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
}
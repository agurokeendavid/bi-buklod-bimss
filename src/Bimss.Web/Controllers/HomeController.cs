using System.Diagnostics;
using Bimss.Infrastructure.ExceptionHandling;
using Bimss.Web.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Bimss.Web.Controllers;

public class HomeController : Controller
{
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
        var exception = HttpContext.Features.Get<IExceptionHandlerPathFeature>()?.Error;
        var classification = exception is not null
            ? ExceptionClassifier.Classify(exception)
            : new ExceptionClassification(StatusCodes.Status500InternalServerError, "An unexpected error occurred.", string.Empty);

        Response.StatusCode = classification.StatusCode;

        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = classification.StatusCode,
            Title = classification.Title,
            Detail = classification.Detail,
        });
    }
}

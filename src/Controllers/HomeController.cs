using Microsoft.AspNetCore.StaticFiles;

namespace AiFoundryAgent.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        // In all environments, serve the React app's index.html
        // The React app handles routing internally via client-side routing
        var indexPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");
        if (System.IO.File.Exists(indexPath))
        {
            return PhysicalFile(indexPath, "text/html");
        }
        else
        {
            // If index.html doesn't exist (not built yet), return a basic error page
            return Content("React app not found. Please run 'npm run build' in the client-app directory.", "text/html");
        }
    }
}
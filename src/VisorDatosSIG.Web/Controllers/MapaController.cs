using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VisorDatosSIG.Web.Controllers;

[Authorize]
public class MapaController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}

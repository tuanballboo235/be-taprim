using Microsoft.AspNetCore.Mvc;

namespace TAPrim.API.Controllers
{
	public class TelegramBotController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}

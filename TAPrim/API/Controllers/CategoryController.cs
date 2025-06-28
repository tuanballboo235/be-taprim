using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TAPrim.Application.DTOs.ProductAccounts;
using TAPrim.Application.Services;
using TAPrim.Application.Services.ServiceImpl;
using TAPrim.Common.Helpers;

namespace TAPrim.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CategoryController : ControllerBase
	{
		private readonly ICategoryService _categoryService;
		public CategoryController(ICategoryService categoryService)
		{
			_categoryService = categoryService;
		}
		[HttpPost("get-category-tree")]
		public IActionResult GetCategoryTree()
		{
			return Ok( _categoryService.GetCategoryTreeAsync());
		}
	}
}

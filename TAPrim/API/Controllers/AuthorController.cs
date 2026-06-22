using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TAPrim.Application.DTOs.Auth;
using TAPrim.Application.Services;

namespace TAPrim.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthorController : ControllerBase
	{
		private readonly IJwtSService _jwtService;

		public AuthorController(IJwtSService jwtService)
		{
			_jwtService = jwtService;
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginDto dto)
		{
			var account = await _au.LoginAsync(dto.Email, dto.Password);

			if (account == null)
			{
				return Unauthorized("Email hoặc mật khẩu không đúng");
			}

			var token = _jwtService.GenerateToken(account);
			
			return Ok(new
			{
				accessToken = token,
				user = new
				{
					account.Id,
					account.Email,
					account.FullName
				}
			});
		}

	}
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TAPrim.Application.DTOs.Auth;
using TAPrim.Application.Services;
using TAPrim.Models;

namespace TAPrim.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;
		private readonly IJwtSService _jwtService;

		public AuthController(IAuthService authService, IJwtSService jwtService)
		{
			_authService = authService;
			_jwtService = jwtService;
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginDto dto)
		{
			var loginName = string.IsNullOrWhiteSpace(dto.Username)
				? dto.Email
				: dto.Username;

			if (string.IsNullOrWhiteSpace(loginName) || string.IsNullOrWhiteSpace(dto.Password))
			{
				return BadRequest(new { message = "Vui lòng nhập tên đăng nhập và mật khẩu." });
			}

			var account = await _authService.LoginAsync(loginName, dto.Password);

			if (account == null)
			{
				return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng." });
			}

			return Ok(new
			{
				accessToken = _jwtService.GenerateToken(account),
				tokenType = "Bearer",
				user = MapUser(account)
			});
		}

		[Authorize]
		[HttpGet("me")]
		public async Task<IActionResult> Me()
		{
			var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (!int.TryParse(idClaim, out var userId))
			{
				return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });
			}

			var account = await _authService.GetByIdAsync(userId);

			if (account == null)
			{
				return Unauthorized(new { message = "Tài khoản khong ton tai hoac da bi khoa." });
			}

			return Ok(new { user = MapUser(account) });
		}

		private static object MapUser(User account)
		{
			return new
			{
				id = account.UserId,
				username = account.Username,
				phone = account.Phone,
				role = account.Role
			};
		}
	}
}

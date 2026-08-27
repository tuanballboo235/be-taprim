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


		[Authorize]
		[HttpPut("me")]
		public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequestDto request)
		{
			var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (!int.TryParse(idClaim, out var userId))
			{
				return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });
			}

			var result = await _authService.UpdateProfileAsync(userId, request);
			if (!result.Success)
			{
				return BadRequest(new { message = result.Message });
			}

			return Ok(new
			{
				message = result.Message,
				user = result.User
			});
		}

		[Authorize]
		[HttpPut("change-password")]
		public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
		{
			var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (!int.TryParse(idClaim, out var userId))
			{
				return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });
			}

			if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
			{
				return BadRequest(new { message = "Vui lòng nhập đầy đủ mật khẩu hiện tại và mật khẩu mới." });
			}

			if (request.NewPassword.Trim().Length < 6)
			{
				return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 6 ký tự." });
			}

			if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
			{
				return BadRequest(new { message = "Xác nhận mật khẩu mới không khớp." });
			}

			var changed = await _authService.ChangePasswordAsync(
				userId,
				request.CurrentPassword,
				request.NewPassword.Trim());

			if (!changed)
			{
				return BadRequest(new { message = "Mật khẩu hiện tại không đúng hoặc tài khoản không còn hợp lệ." });
			}

			return Ok(new { message = "Đổi mật khẩu thành công." });
		}

		[HttpPost("send-code")]
		public async Task<IActionResult> SendCode([FromBody] SendCodeDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Email))
			{
				return BadRequest(new { message = "Vui lòng nhập địa chỉ email." });
			}

			if (!dto.Email.Contains("@"))
			{
				return BadRequest(new { message = "Địa chỉ email không hợp lệ." });
			}

			var trimmedEmail = dto.Email.Trim().ToLower();
			var userExists = await _authService.CheckUserExistsAsync(trimmedEmail);

			if (string.Equals(dto.Purpose, "register", StringComparison.OrdinalIgnoreCase) && userExists)
			{
				return BadRequest(new { message = "Email này đã được đăng ký tài khoản khác." });
			}

			if (string.Equals(dto.Purpose, "forgot", StringComparison.OrdinalIgnoreCase) && !userExists)
			{
				return BadRequest(new { message = "Tài khoản không tồn tại hoặc đã bị khóa." });
			}

			var success = await _authService.SendVerificationCodeAsync(dto.Email);
			if (!success)
			{
				return BadRequest(new { message = "Gửi mã xác minh thất bại." });
			}

			return Ok(new { message = "Mã xác minh đã được gửi về email của bạn." });
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.Code))
			{
				return BadRequest(new { message = "Vui lòng điền đầy đủ các thông tin bắt buộc." });
			}

			var codeValid = await _authService.VerifyCodeAsync(dto.Email, dto.Code);
			if (!codeValid)
			{
				return BadRequest(new { message = "Mã xác minh không chính xác hoặc đã hết hạn." });
			}

			if (dto.Password.Trim().Length < 6)
			{
				return BadRequest(new { message = "Mật khẩu phải có ít nhất 6 ký tự." });
			}

			var success = await _authService.RegisterAsync(dto.Username.Trim(), dto.Email.Trim(), dto.Password.Trim());
			if (!success)
			{
				// In Vietnamese: "Tên đăng nhập hoặc Email này đã được đăng ký tài khoản khác."
				return BadRequest(new { message = "Tên đăng nhập hoặc Email này đã được đăng ký tài khoản khác." });
			}

			return Ok(new { message = "Đăng ký tài khoản thành công." });
		}

		[HttpPost("forgot-password")]
		public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.NewPassword) || string.IsNullOrWhiteSpace(dto.Code))
			{
				return BadRequest(new { message = "Vui lòng điền đầy đủ các thông tin bắt buộc." });
			}

			var codeValid = await _authService.VerifyCodeAsync(dto.Email, dto.Code);
			if (!codeValid)
			{
				return BadRequest(new { message = "Mã xác minh không chính xác hoặc đã hết hạn." });
			}

			if (dto.NewPassword.Trim().Length < 6)
			{
				return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 6 ký tự." });
			}

			var success = await _authService.ResetPasswordAsync(dto.Email, dto.NewPassword.Trim());
			if (!success)
			{
				return BadRequest(new { message = "Tài khoản không tồn tại hoặc đã bị khóa." });
			}

			return Ok(new { message = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại." });
		}

		private static object MapUser(User account)
		{
			return new
			{
				id = account.UserId,
				username = account.Username,
				phone = account.Phone,
				
				email = account.Email,
				role = account.Role
			};
		}
	}
}

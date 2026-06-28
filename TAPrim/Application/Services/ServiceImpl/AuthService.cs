using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using BasketballAcademyManagementSystemAPI.Common.Helpers;
using TAPrim.Application.Services;
using TAPrim.Models;

namespace TAPrim.Application.Services.ServiceImpl
{
	public class AuthService : IAuthService
	{
		private readonly TaprimContext _context;
		private readonly IMemoryCache _cache;
		private readonly EmailHelper _emailHelper;

		public AuthService(TaprimContext context, IMemoryCache cache, EmailHelper emailHelper)
		{
			_context = context;
			_cache = cache;
			_emailHelper = emailHelper;
		}

		public async Task<User?> LoginAsync(string usernameOrEmail, string password)
		{
			var loginName = usernameOrEmail.Trim();

			if (string.IsNullOrWhiteSpace(loginName) || string.IsNullOrEmpty(password))
			{
				return null;
			}

			var user = await _context.Users
				.AsNoTracking()
				.FirstOrDefaultAsync(u => (u.Username == loginName || u.Email == loginName) && u.IsEnable);

			if (user == null || !string.Equals(user.Password, password, StringComparison.Ordinal))
			{
				return null;
			}

			return user;
		}

		public Task<User?> GetByIdAsync(int userId)
		{
			return _context.Users
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.UserId == userId && u.IsEnable);
		}

		public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
		{
			if (userId <= 0 || string.IsNullOrEmpty(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
			{
				return false;
			}

			var user = await _context.Users
				.FirstOrDefaultAsync(u => u.UserId == userId && u.IsEnable);

			if (user == null || !string.Equals(user.Password, currentPassword, StringComparison.Ordinal))
			{
				return false;
			}

			user.Password = newPassword;
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> SendVerificationCodeAsync(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
			{
				return false;
			}

			var code = new Random().Next(100000, 999999).ToString();
			var cacheKey = $"VerificationCode_{email.Trim().ToLower()}";
			_cache.Set(cacheKey, code, TimeSpan.FromMinutes(5));

			var subject = "Mã xác minh tài khoản TAPRIM Shop";
			var body = $@"
				<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 8px;'>
					<h2 style='color: #16a34a; text-align: center;'>Xác minh tài khoản TAPRIM</h2>
					<p>Xin chào,</p>
					<p>Bạn đã yêu cầu mã xác minh (OTP) để thực hiện đăng ký hoặc đặt lại mật khẩu tại TAPRIM Shop.</p>
					<div style='background-color: #f0fdf4; border: 1px dashed #16a34a; border-radius: 6px; padding: 15px; text-align: center; margin: 20px 0;'>
						<span style='font-size: 24px; font-weight: bold; color: #16a34a; letter-spacing: 4px;'>{code}</span>
					</div>
					<p>Mã này có hiệu lực trong vòng <strong>5 phút</strong>. Vui lòng không chia sẻ mã này cho bất kỳ ai khác.</p>
					<hr style='border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;' />
					<p style='font-size: 12px; color: #64748b; text-align: center;'>Đây là email tự động, vui lòng không trả lời email này.</p>
				</div>";

			await EmailHelper.SendEmailMultiThreadAsync(email, subject, body);
			return true;
		}

		public async Task<bool> VerifyCodeAsync(string email, string code)
		{
			if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
			{
				return false;
			}

			var cacheKey = $"VerificationCode_{email.Trim().ToLower()}";
			if (_cache.TryGetValue(cacheKey, out string? cachedCode))
			{
				if (string.Equals(cachedCode, code.Trim(), StringComparison.Ordinal))
				{
					_cache.Remove(cacheKey);
					return true;
				}
			}

			return await Task.FromResult(false);
		}

		public async Task<bool> RegisterAsync(string username, string email, string password)
		{
			if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
			{
				return false;
			}

			var trimmedUsername = username.Trim().ToLower();
			var trimmedEmail = email.Trim().ToLower();
			var exists = await _context.Users.AnyAsync(u => u.Username == trimmedUsername || u.Email == trimmedEmail);
			if (exists)
			{
				return false;
			}

			var user = new User
			{
				Username = trimmedUsername,
				Email = trimmedEmail,
				Password = password,
				Role = "client",
				IsEnable = true,
				CreateAt = DateTime.Now
			};

			_context.Users.Add(user);
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> ResetPasswordAsync(string email, string newPassword)
		{
			if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(newPassword))
			{
				return false;
			}

			var trimmedEmail = email.Trim().ToLower();
			var user = await _context.Users.FirstOrDefaultAsync(u => (u.Email == trimmedEmail || u.Username == trimmedEmail) && u.IsEnable);
			if (user == null)
			{
				return false;
			}

			user.Password = newPassword;
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> CheckUserExistsAsync(string emailOrUsername)
		{
			if (string.IsNullOrWhiteSpace(emailOrUsername))
			{
				return false;
			}

			var name = emailOrUsername.Trim().ToLower();
			return await _context.Users.AnyAsync(u => (u.Username == name || u.Email == name) && u.IsEnable);
		}
	}
}

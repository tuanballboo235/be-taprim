using TAPrim.Models;

namespace TAPrim.Application.Services
{
	public interface IAuthService
	{
		Task<User?> LoginAsync(string usernameOrEmail, string password);

		Task<User?> GetByIdAsync(int userId);

		Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

		Task<bool> SendVerificationCodeAsync(string email);

		Task<bool> VerifyCodeAsync(string email, string code);

		Task<bool> RegisterAsync(string username, string email, string password);

		Task<bool> ResetPasswordAsync(string email, string newPassword);

		Task<bool> CheckUserExistsAsync(string emailOrUsername);
	}
}

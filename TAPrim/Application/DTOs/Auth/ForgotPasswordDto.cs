namespace TAPrim.Application.DTOs.Auth
{
	public class ForgotPasswordDto
	{
		public string Email { get; set; } = null!;
		public string NewPassword { get; set; } = null!;
		public string Code { get; set; } = null!;
	}
}

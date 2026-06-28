namespace TAPrim.Application.DTOs.Auth
{
	public class RegisterDto
	{
		public string Email { get; set; } = null!;
		public string Username { get; set; } = null!;
		public string Password { get; set; } = null!;
		public string Code { get; set; } = null!;
	}
}

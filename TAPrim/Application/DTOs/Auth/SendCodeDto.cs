namespace TAPrim.Application.DTOs.Auth
{
	public class SendCodeDto
	{
		public string Email { get; set; } = null!;
		public string? Purpose { get; set; }
	}
}

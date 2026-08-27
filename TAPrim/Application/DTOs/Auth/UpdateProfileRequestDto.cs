namespace TAPrim.Application.DTOs.Auth
{
	public class UpdateProfileRequestDto
	{
		public string? Username { get; set; }
		public string? Email { get; set; }
		public string? Phone { get; set; }
	}
}
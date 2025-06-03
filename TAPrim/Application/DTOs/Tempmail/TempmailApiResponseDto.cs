using Newtonsoft.Json;

namespace TAPrim.Application.DTOs.Tempmail
{
	public class TempmailApiResponseDto
	{
		[JsonProperty("success")]
		public bool Success { get; set; }
		[JsonProperty("message")]
		public string Message { get; set; }
		[JsonProperty("data")]
		public TempmailDataDto Data { get; set; }
	}
}

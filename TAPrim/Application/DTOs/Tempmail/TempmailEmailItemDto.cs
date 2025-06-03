using Newtonsoft.Json;

namespace TAPrim.Application.DTOs.Tempmail
{
	public class TempmailEmailItemDto
	{
		[JsonProperty("id")]
		public int Id { get; set; }
		[JsonProperty("sender_name")]
		public string SenderName { get; set; }
		[JsonProperty("from")]
		public string From { get; set; }
		[JsonProperty("to")]
		public string To { get; set; }
		[JsonProperty("subject")]
		public string Subject { get; set; }
		[JsonProperty("read_at")]
		public DateTime? ReadAt { get; set; }
		[JsonProperty("created_at")]
		public DateTime CreatedAt { get; set; }
	}
}

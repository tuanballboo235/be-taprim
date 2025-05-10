using TAPrim.Shared.Constants;

namespace TAPrim.Application.DTOs
{
    public class ApiResponseModel<T>
    {
        public string Status { get; set; } = ApiResponseStatusConstant.FailedStatus;
        public string? Message { get; set; }
        public T? Data { get; set; }
        public Dictionary<string, string>? Errors { get; set; }
    }
}

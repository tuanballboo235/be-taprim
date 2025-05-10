using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text.Json;
using TAPrim.Application.DTOs;
using TAPrim.Application.DTOs.Netflix;
using TAPrim.Shared.Constants;

namespace TAPrim.Application.ServiceImpl
{
	public class NetflixService : INetflixService
	{
		private readonly HttpClient _httpClient;
		public NetflixService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		/*
		 Hàm lấy ra nội dung email từ api trang https://hunght1890.com/
		 */
		public async Task<ApiResponseModel<List<EmailResponseDto>>> GetJsonDataAsync(string email)
		{
			// Lọc dữ liệu email từ phương thức FilterEmailNetflix
			var dataFiltered = await FilterEmailNetflix(email);
			return dataFiltered; // Trả về kết quả đã lọc
		}

		private async Task<ApiResponseModel<List<EmailResponseDto>>> FilterEmailNetflix(string email)
		{
			var apiResponse = new ApiResponseModel<List<EmailResponseDto>>();

			try
			{
				var url = $"https://hunght1890.com/{email}";
				var response = await _httpClient.GetAsync(url);

				if (response.IsSuccessStatusCode)
				{
					var data = await response.Content.ReadAsStringAsync();

					try
					{
						// Deserialize directly into List<EmailResponseDto> if the JSON is an array
						var emailList = JsonConvert.DeserializeObject<List<EmailResponseDto>>(data);

						if (emailList != null)
						{
							var filteredEmails = emailList
								.Where(email => email?.Subject != null && email.Subject.Contains("Mã truy cập Netflix tạm thời của bạn"))
								.ToList();

							apiResponse.Status = ApiResponseStatusConstant.SuccessStatus;
							apiResponse.Data = filteredEmails ?? new List<EmailResponseDto>();
						}
						else
						{
							apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
							apiResponse.Message = "No data found.";
						}
					}
					catch (System.Text.Json.JsonException ex)
					{
						apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
						apiResponse.Message = $"JSON deserialization error: {ex.Message}";
					}
				}
				else
				{
					apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
					apiResponse.Message = $"Error: {response.ReasonPhrase}";
				}
			}
			catch (HttpRequestException ex)
			{
				apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
				apiResponse.Message = $"HttpRequestException: {ex.Message}";
			}
			catch (Exception ex)
			{
				apiResponse.Status = ApiResponseStatusConstant.FailedStatus;
				apiResponse.Message = $"Unexpected error: {ex.Message}";
			}

			return apiResponse;
		}


	}
}


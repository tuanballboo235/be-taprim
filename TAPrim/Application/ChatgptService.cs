using OtpNet;
using TAPrim.Application.DTOs;
using TAPrim.Application.DTOs.Chatgpt;
using TAPrim.Shared.Constants;

namespace TAPrim.Application
{
	public class ChatgptService : IChatgptService
	{
		public async Task<ApiResponseModel<Chatgpt2FaDto>> GetChatgptOtp(string paymentCode, string secretCode)
		{
		
			if (string.IsNullOrWhiteSpace(secretCode))
			{
				return new ApiResponseModel<Chatgpt2FaDto>
				{

					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Mã bảo mật không được để trống.",
					Errors = new Dictionary<string, string>()
					{
						{ "nullSecretCode", "Secret không được để trống." }
					}
				};
			}

			try
			{
				// Giải mã base32 secret
				byte[] secretBytes = Base32Encoding.ToBytes(secretCode);

				// Tạo TOTP từ secret
				var totp = new Totp(secretBytes);

				
					var otp = totp.ComputeTotp(); // Mã OTP hiện tại (6 số)
					var remainingSeconds = totp.RemainingSeconds(); // Thời gian còn lại
					return new ApiResponseModel<Chatgpt2FaDto>
					{

						Status = ApiResponseStatusConstant.SuccessStatus,
						Message = "Lấy thành công mã",
						Data = new Chatgpt2FaDto
						{
							Otp = otp,
							RemainingSeconds = remainingSeconds
						}
					};
				
			}
			catch (Exception ex)
			{
				return new ApiResponseModel<Chatgpt2FaDto>
				{

					Status = ApiResponseStatusConstant.FailedStatus,
					Message = "Mã bảo mật không được để trống.",
					Errors = new Dictionary<string, string>()
					{
						{ "nullSecretCode", "Secret không được để trống." }
					}
				};
			}

		}
	}
}

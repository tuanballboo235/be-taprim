using TAPrim.Infrastructure.Repositories;

namespace TAPrim.Application.Services.ServiceImpl
{
	public class SendMailService:ISendMailService
	{
		private readonly ISendMailRepository  _sendMailRepository;
		public SendMailService(ISendMailRepository sendMailRepository)
		{
			_sendMailRepository = sendMailRepository;
		}

		public Task SendMailByMailTemplateIdAsync(string mailTemplateId, string email, string urlToPage, dynamic data)
		{
			var mailTemplate = await _mailTemplateRepository.GetByIdAsync(mailTemplateId);
			if (mailTemplate != null)
			{
				switch (mailTemplateId)
				{
					//sửa nội dung approve manager registration
					case string id when id == MailTemplateConstant.ApproveManagerRegistration:
						mailTemplate.Content = mailTemplate.Content
							.Replace("{{USER_NAME}}", data?.Fullname ?? "Bạn")
							.Replace("{{ROLE_CODE}}", data?.RoleCode ?? "Học Viên")
							.Replace("{{URL_GO_TO_PAGE}}", urlToPage);
						break;
				
					case null:
						throw new ArgumentNullException(nameof(mailTemplateId), "mailTemplateId cannot be null.");

					default:
						throw new ArgumentException("Invalid mailTemplateId", nameof(mailTemplateId));
				}

				var t = new Thread(() => _emailHelper.SendEmailMultiThread(email, mailTemplate.TemplateTitle, mailTemplate.Content));
				t.Start();
			}

		}
	}
}

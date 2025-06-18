using BasketballAcademyManagementSystemAPI.Common.Helpers;
using TAPrim.Infrastructure.Repositories;
using TAPrim.Shared.Constants;

namespace TAPrim.Application.Services.ServiceImpl
{
	public class SendMailService:ISendMailService
	{
		private readonly ISendMailRepository  _sendMailRepository;
		private readonly EmailHelper _emailHelper;
		public SendMailService(ISendMailRepository sendMailRepository, EmailHelper emailHelper)
		{
			_sendMailRepository = sendMailRepository;
			_emailHelper = emailHelper;
		}

		public async Task SendMailByMailTemplateIdAsync(string mailTemplateId, string email, dynamic data)
		{

			var mailTemplate = await _sendMailRepository.GetByIdAsync(mailTemplateId);
			if (mailTemplate != null)
			{
				//replace default email section
				mailTemplate.Content = mailTemplate.Content
					.Replace("{{FACEBOOK_PAGE_URL}}", MailTemplateConstant.FacebookPageURL);
				//replace theo từng email
				switch (mailTemplateId)
				{
					//sửa nội dung approve manager registration
					case string id when id == MailTemplateConstant.PaymentSucess:
						mailTemplate.Content = mailTemplate.Content
							.Replace("{{PAYMENT_TRANSACTION_CODE}}", data?.TransactionCode ?? "N/A")
							.Replace("{{TRANSACTION_DATE}}", data?.TransactionDate ?? "N/A")
							.Replace("{{TRANSACTION_STATUS}}", data?.Status ?? "N/A")
							.Replace("{{ORDER_TRACKING_URL}}", "http://103.238.235.227:8080/");
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

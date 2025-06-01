using Microsoft.EntityFrameworkCore;
using TAPrim.Infrastructure.Repositories;

namespace TAPrim.Shared.Helpers
{
	public class TransactionCodeHelper
	{
		private readonly IPaymentRepository _paymentRepository;
		public TransactionCodeHelper(IPaymentRepository paymentRepository) {
			_paymentRepository = paymentRepository; 
		}
		private static readonly Random _random = new Random();
		private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

		// tạo ngẫu nhiên mã thanh toán : TA XXXX 
		public static string GenerateTransactionCode()
		{
			var chars = new char[4];
			for (int i = 0; i < 4; i++)
				chars[i] = Chars[_random.Next(Chars.Length)];
			return $"TP {new string(chars)}";
		}
		public async Task<string> GetCode()
		{
			string transactionCode;
			do
			{
				transactionCode = GenerateTransactionCode();
			} while (await _paymentRepository.IsExistedTransactionCode(transactionCode)); // tạo mời transaction code và kiểm tra có trùng lặp code trong db hay chưa
			return transactionCode;
		}

		

	}
}

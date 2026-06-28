using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TAPrim.Application.DTOs.Common;
using TAPrim.Application.DTOs.Payment;
using TAPrim.Application.DTOs.ProductAccounts;
using TAPrim.Models;
using TAPrim.Shared.Constants;

namespace TAPrim.Infrastructure.Repositories.RepositoryImpl
{
    public class ProductAccountRepository : IProductAccountRepository
	{
		private readonly TaprimContext _context;

		public ProductAccountRepository(TaprimContext context)
		{
			_context = context;
		}

		public async Task<Product?> GetProductByIdAsync(int productId)
		{
			return await _context.Products.FindAsync(productId);
		}

        public async Task<ProductOption?> GetProductOptionByIdAsync(int productOptionId)
        {
            return await _context.ProductOptions.FindAsync(productOptionId);
        }

		public async Task<ProductAccount?> GetProductAccountByIdAsync(int productAccountId)
		{
			return await _context.ProductAccounts.FindAsync(productAccountId);
		}

		public async Task AddProductAccountAsync(ProductAccount account)
		{
			_context.ProductAccounts.Add(account);
			await _context.SaveChangesAsync();
		}

		public async Task<PagedResponseDto<ProductAccount>> GetFilteredProductAccountsAsync(ProductAccountQueryDto query)
		{
			// Nếu query null, khởi tạo mặc định
			query ??= new ProductAccountQueryDto
			{
				PageIndex = 1,
				PageSize = 100000
			};

			var q = _context.ProductAccounts.AsQueryable();
			if (query.ProductOptionId > 0)
			{
				q = q.Where(pa => pa.ProductOptionId == query.ProductOptionId);
			}

			if (!string.IsNullOrEmpty(query.Username))
			{
				q = q.Where(pa => pa.UsernameProductAccount.Contains(query.Username));
			}

			if (query.Status.HasValue)
			{
				q = q.Where(pa => pa.Status == query.Status);
			}

			if (query.FromDateChangePass.HasValue)
			{
				q = q.Where(pa => pa.DateChangePass >= query.FromDateChangePass);
			}

			if (query.ToDateChangePass.HasValue)
			{
				q = q.Where(pa => pa.DateChangePass <= query.ToDateChangePass);
			}

			if (query.MinSellCount.HasValue)
			{
				q = q.Where(pa => pa.SellCount >= query.MinSellCount);
			}

			if (query.MaxSellCount.HasValue)
			{
				q = q.Where(pa => pa.SellCount <= query.MaxSellCount);
			}
			if (query.CanSell.HasValue)
			{
				if (query.CanSell.Value)
				{
					q = q.Where(pa =>
						pa.Status == ProductAccountStatusConstant.Available &&
						pa.SellCount > 0 &&
						pa.SellFrom < DateTime.Now &&
						pa.SellTo > DateTime.Now);
				}
				else
				{
					q = q.Where(pa =>
						pa.Status != ProductAccountStatusConstant.Available ||
						pa.SellCount <= 0 ||
						pa.SellFrom >= DateTime.Now ||
						pa.SellTo <= DateTime.Now);
				}
			}

			var totalRecords = await q.CountAsync();
			var totalPages = (int)Math.Ceiling((double)totalRecords / query.PageSize);

			var items = await q
				.OrderByDescending(pa => pa.DateChangePass)
				.Skip((query.PageIndex - 1) * query.PageSize)
				.Take(query.PageSize)
				.ToListAsync();

			return new PagedResponseDto<ProductAccount>
			{
				Items = items,
				TotalRecords = totalRecords,
				TotalPages = totalPages,
				CurrentPage = query.PageIndex,
				PageSize = query.PageSize
			};
		}

		//lấy ra số lượng product Account dựa vào productId
		public async Task<int> GetQuantityStockProductAccountByProductOptionId(int ProductOptionId)
		{
			int totalSellCount = await _context.ProductAccounts
								.Where(x => x.ProductOptionId == ProductOptionId)
								.SumAsync(x => x.SellCount ?? 0);
			return totalSellCount;
		}

		public async Task<List<ProductAccount>> GetListProductAccountByProductOptionId(int productOptionId)
		{
			var productAccountList = await _context.ProductAccounts.Where(x => x.ProductOptionId == productOptionId &&
														x.Status == ProductAccountStatusConstant.Available && // lấy ra account đc kích hoạt
														x.SellCount >0 // lấy ra lượt bán > 0
														).ToListAsync();
			return productAccountList;
		}

		public async Task<ProductAccountResponseDto?> GetProductAccountByPaymentTransactionCode(string transactionCode)
		{
			var payment = await _context.Payments
				.Include(x => x.Order)
				.ThenInclude(x => x.ProductAccount)
				.FirstOrDefaultAsync(x => x.TransactionCode == transactionCode);

			var order = payment?.Order;
			if (order == null)
			{
				return null;
			}

			var reservationItems = TryReadReservationMetadata(order.ClientNote)?.Items
				.Where(x => x.ProductAccountId > 0 && x.Quantity > 0)
				.ToList() ?? new List<PaymentReservationItem>();

			if (reservationItems.Count == 0 && order.ProductAccountId.HasValue)
			{
				reservationItems.Add(new PaymentReservationItem
				{
					ProductAccountId = order.ProductAccountId.Value,
					Quantity = 1
				});
			}

			var accountIds = reservationItems.Select(x => x.ProductAccountId).Distinct().ToList();
			var accounts = await _context.ProductAccounts
				.Where(x => accountIds.Contains(x.ProductAccountId))
				.ToListAsync();

			var accountDataLines = new List<string>();
			foreach (var item in reservationItems)
			{
				var account = accounts.FirstOrDefault(x => x.ProductAccountId == item.ProductAccountId);
				if (account == null) continue;

				var line = GetAccountDisplayValue(account);
				for (var i = 0; i < item.Quantity; i++)
				{
					accountDataLines.Add(line);
				}
			}

			var firstAccount = accounts.FirstOrDefault();
			return new ProductAccountResponseDto
			{
				ProductAccountId = firstAccount?.ProductAccountId,
				ProductOptionId = order.ProductOptionId,
				AccountData = accountDataLines.Count > 0
					? string.Join(Environment.NewLine, accountDataLines)
					: firstAccount?.AccountData,
				UsernameProductAccount = accounts.Count == 1 ? firstAccount?.UsernameProductAccount : null,
				PasswordProductAccount = accounts.Count == 1 ? firstAccount?.PasswordProductAccount : null,
				Status = firstAccount?.Status ?? 0
			};
		}
		private static string GetAccountDisplayValue(ProductAccount account)
		{
			if (!string.IsNullOrWhiteSpace(account.AccountData))
			{
				return account.AccountData.Trim();
			}

			var username = account.UsernameProductAccount?.Trim();
			var password = account.PasswordProductAccount?.Trim();
			return string.IsNullOrWhiteSpace(password) ? username ?? string.Empty : $"{username}:{password}";
		}

		private static PaymentReservationMetadata? TryReadReservationMetadata(string? clientNote)
		{
			if (string.IsNullOrWhiteSpace(clientNote))
			{
				return null;
			}

			try
			{
				return JsonSerializer.Deserialize<PaymentReservationMetadata>(clientNote);
			}
			catch (JsonException)
			{
				return null;
			}
		}

		public async Task<bool> UpdateProductAccount(ProductAccount productAccount)
		{

			_context.ProductAccounts.Update(productAccount);
			return await _context.SaveChangesAsync() > 0;

		}

		public async Task<int> GetTotalSellCountByProductOptionIdAsync(int productOptionId)
		{
			return await _context.ProductAccounts
				.Where(pa => pa.ProductOptionId == productOptionId)
				.SumAsync(pa => pa.SellCount ?? 0); // đề phòng sellCount null
		}

		//public async Task<int> DeleteProductAccount(int )
		//{
		//	return await _context.ProductAccounts
		//		.Where(pa => pa.ProductOptionId == productOptionId)
		//		.SumAsync(pa => pa.SellCount ?? 0); // đề phòng sellCount null
		//}


	}
}

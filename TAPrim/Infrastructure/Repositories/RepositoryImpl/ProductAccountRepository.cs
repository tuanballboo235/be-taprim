using Microsoft.EntityFrameworkCore;
using TAPrim.Application.DTOs.Common;
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
			var q = _context.ProductAccounts.AsQueryable();

			// Nếu query null, khởi tạo mặc định
			query ??= new ProductAccountQueryDto
			{
				PageIndex = 1,
				PageSize = 100000
			};

			if (query.ProductId > 0)
			{
				q = q.Where(pa => pa.ProductId == query.ProductId);
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
		public async Task<int> GetQuantityStockProductAccountByProductId(int productId)
		{
			int totalSellCount = await _context.ProductAccounts
								.Where(x => x.ProductId == productId)
								.SumAsync(x => x.SellCount ?? 0);
			return totalSellCount;
		}

		public async Task<List<ProductAccount>> GetListProductAccountByProductId(int productId)
		{
			return await _context.ProductAccounts.Where(x => x.ProductId == productId &&
														x.Status != ProductAccountStatusConstant.Unavailable 
														).ToListAsync();
		}

		public async Task<ProductAccountResponseDto?> GetProductAccountByPaymentTransactionCode(string transactionCode)
		{

			// 2. Tìm order 
			var order = await _context.Orders.Include(x => x.Payment)
				.Where(x => x.Payment.TransactionCode == transactionCode)
				.FirstOrDefaultAsync();

			var productAccount = await _context.ProductAccounts
				.Where(x => x.ProductAccountId == order.ProductAccountId)
				.FirstOrDefaultAsync();

			var responseDto = new ProductAccountResponseDto
			{
				AccountData = productAccount.AccountData,
				UsernameProductAccount = productAccount.UsernameProductAccount,
				PasswordProductAccount = productAccount.PasswordProductAccount,
				// Map other properties here as needed
			};

			return responseDto;
		}

		public async Task<bool> UpdateProductAccount(ProductAccount productAccount)
		{

			_context.ProductAccounts.Update(productAccount);
			return await _context.SaveChangesAsync() > 0;

		}

		public async Task<List<ProductAccount?>> GetProductAccountByProductId(int productId)
		{
			return await _context.ProductAccounts.Where(x => x.ProductId == productId).ToListAsync();
		}
	}
}

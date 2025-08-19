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
														x.Status != ProductAccountStatusConstant.Unavailable && // lấy ra account đc kích hoạt
														x.SellCount >0 // lấy ra lượt bán > 0
														).ToListAsync();
			return productAccountList;
		}

		public async Task<ProductAccountResponseDto?> GetProductAccountByPaymentTransactionCode(string transactionCode)
		{

		

			var productAccount = await _context.Payments.Include(x=>x.Order).ThenInclude(x=>x.ProductAccount)
				.Where(x => x.TransactionCode == transactionCode).Select(x=>new ProductAccountResponseDto
				{
					AccountData = x.Order.ProductAccount.AccountData,
					UsernameProductAccount = x.Order.ProductAccount.UsernameProductAccount,
					PasswordProductAccount = x.Order.ProductAccount.PasswordProductAccount
				})
				.FirstOrDefaultAsync();

			return productAccount;
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

		public async Task<int> DeleteProductAccount()
		{
			return await _context.ProductAccounts
				.Where(pa => pa.ProductOptionId == productOptionId)
				.SumAsync(pa => pa.SellCount ?? 0); // đề phòng sellCount null
		}


	}
}

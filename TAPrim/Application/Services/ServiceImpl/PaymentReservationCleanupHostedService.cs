namespace TAPrim.Application.Services.ServiceImpl
{
	public class PaymentReservationCleanupHostedService : BackgroundService
	{
		private static readonly TimeSpan CleanupInterval = TimeSpan.FromSeconds(30);
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<PaymentReservationCleanupHostedService> _logger;

		public PaymentReservationCleanupHostedService(
			IServiceProvider serviceProvider,
			ILogger<PaymentReservationCleanupHostedService> logger)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using var scope = _serviceProvider.CreateScope();
					var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
					await paymentService.ReleaseExpiredPendingReservationsAsync();
				}
				catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
				{
					break;
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "Không thể dọn đơn thanh toán tạm đã hết hạn.");
				}

				await Task.Delay(CleanupInterval, stoppingToken);
			}
		}
	}
}

using OtpNet;

Console.WriteLine("🔐 Nhập secret key (Base32): ");
string? secret = Console.ReadLine();

if (string.IsNullOrWhiteSpace(secret))
{
	Console.WriteLine("❌ Secret không được để trống.");
	return;
}

try
{
	// Giải mã base32 secret
	byte[] secretBytes = Base32Encoding.ToBytes(secret);

	// Tạo TOTP từ secret
	var totp = new Totp(secretBytes);

	while (true)
	{
		var otp = totp.ComputeTotp(); // Mã OTP hiện tại (6 số)
		var remaining = totp.RemainingSeconds(); // Thời gian còn lại

		Console.Clear();
		Console.WriteLine($"🔑 Secret: {secret}");
		Console.WriteLine($"✅ Mã OTP hiện tại: {otp}");
		Console.WriteLine($"⏳ Thời gian còn lại: {remaining} giây");

		Thread.Sleep(1000); // làm mới mỗi giây
	}
}
catch (Exception ex)
{
	Console.WriteLine($"❌ Lỗi: {ex.Message}");
}

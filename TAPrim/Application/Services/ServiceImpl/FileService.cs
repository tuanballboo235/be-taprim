namespace TAPrim.Application.Services.ServiceImpl
{
	public class FileService : IFileService
	{
		private readonly IWebHostEnvironment _env;
		private readonly string[] _allowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
		private const long MaxFileSize = 2 * 1024 * 1024; // 2MB

		public FileService(IWebHostEnvironment env)
		{
			_env = env;
		}

		public async Task<string> SaveImageAsync(IFormFile file)


		{
			if (file == null || file.Length == 0)
				throw new Exception("File không hợp lệ.");

			var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
			if (!_allowedExtensions.Contains(ext))
				throw new Exception("Chỉ chấp nhận ảnh JPG, PNG, WebP.");

			if (file.Length > MaxFileSize)
				throw new Exception("Ảnh không được vượt quá 2MB.");

			var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
			if (!Directory.Exists(uploadsFolder))
				Directory.CreateDirectory(uploadsFolder);

			var fileName = Guid.NewGuid().ToString() + ext;
			var filePath = Path.Combine(uploadsFolder, fileName);

			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}

			// ✅ Chỉ trả về đường dẫn tương đối để gán vào src
			return $"/uploads/{fileName}";
		}

	}
}

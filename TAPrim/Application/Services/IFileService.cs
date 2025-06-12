namespace TAPrim.Application.Services
{
    public interface IFileService
    {
        Task<string> SaveImageAsync(IFormFile file);
        Task DeleteImage(string relativePath);

	}
}

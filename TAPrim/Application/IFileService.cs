namespace TAPrim.Application
{
	public interface IFileService
	{
		Task<string> SaveImageAsync(IFormFile file);
	}
}

namespace BugTracker.Services.Interfaces
{
    public interface IBTFileService
    {


        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);

        public string ConvertByteArrayToFile(byte[] fileData, string extension, int defaultImage);

        public string FormatFileSize(long bytes);

        public string GetFileIcon(string file);
    }
}

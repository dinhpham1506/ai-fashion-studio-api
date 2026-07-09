namespace AiFashionStudio.Platform.Application.Common.Interfaces.IServices;

public interface IFileStorage
{
    // Upload file, trả về public URL.
    Task<string> UploadAsync(string bucket, string objectName, byte[] content, string contentType, CancellationToken cancellationToken = default);
}

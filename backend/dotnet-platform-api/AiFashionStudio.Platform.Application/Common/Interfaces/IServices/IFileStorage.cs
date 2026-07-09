namespace AiFashionStudio.Platform.Application.Common.Interfaces.IServices;

public interface IFileStorage
{
    /// <summary>
/// Uploads a file and returns its public URL.
/// </summary>
/// <param name="bucket">The destination storage bucket.</param>
/// <param name="objectName">The destination object name.</param>
/// <param name="content">The file content.</param>
/// <param name="contentType">The MIME type of the file content.</param>
/// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
/// <returns>The public URL of the uploaded file.</returns>
    Task<string> UploadAsync(string bucket, string objectName, byte[] content, string contentType, CancellationToken cancellationToken = default);
}

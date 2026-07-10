using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using Minio.Exceptions;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Infrastructure.Storage
{
    public sealed class MinioFileStorage : IFileStorage
    {
        private readonly IMinioClient _client;
        private readonly string _publicBaseUrl;

        /// <summary>
        /// Creates a MinIO-backed file storage instance.
        /// </summary>
        /// <param name="options">The configured MinIO settings.</param>
        public MinioFileStorage(IOptions<MinioSettings> options)
        {
            var settings = options.Value;
            _publicBaseUrl = settings.PublicBaseUrl.TrimEnd('/');
            _client = new MinioClient()
                .WithEndpoint(settings.Endpoint)
                .WithCredentials(settings.AccessKey, settings.SecretKey)
                .WithSSL(settings.UseSSL)
                .Build();
        }

        /// <summary>
        /// Uploads content to a bucket and returns its public URL.
        /// </summary>
        /// <param name="bucket">The target bucket name.</param>
        /// <param name="objectName">The object name to store in the bucket.</param>
        /// <param name="content">The byte content to upload.</param>
        /// <param name="contentType">The MIME type of the uploaded content.</param>
        /// <param name="cancellationToken">A token that cancels the upload operation.</param>
        /// <returns>The public URL of the stored object.</returns>
        public async Task<string> UploadAsync(string bucket, string objectName, byte[] content, string contentType, CancellationToken cancellationToken = default)
        {
            var exists = await _client.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(bucket), cancellationToken);

            if (!exists)
            {
                try
                {
                    await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket), cancellationToken);
                }
                catch (MinioException ex) when (IsBucketAlreadyExists(ex))
                {
                    // Concurrent upload created the bucket after BucketExistsAsync returned false.
                }
            }

            using var stream = new MemoryStream(content);
            await _client.PutObjectAsync(new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(content.Length)
                .WithContentType(contentType), cancellationToken);

            return $"{_publicBaseUrl}/{bucket}/{objectName}";
        }

        public Task<string> GetTemporaryUrlAsync(string bucket, string objectName, TimeSpan expiresIn, CancellationToken cancellationToken = default)
            => _client.PresignedGetObjectAsync(new PresignedGetObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectName)
                .WithExpiry((int)expiresIn.TotalSeconds));

        private static bool IsBucketAlreadyExists(MinioException exception)
            => exception.Message.Contains("BucketAlready", StringComparison.OrdinalIgnoreCase)
            || exception.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase)
            || exception.Message.Contains("already owned", StringComparison.OrdinalIgnoreCase);
    }
}

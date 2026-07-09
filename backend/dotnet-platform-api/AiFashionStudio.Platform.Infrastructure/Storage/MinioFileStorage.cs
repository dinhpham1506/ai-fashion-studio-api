using AiFashionStudio.Platform.Application.Common.Interfaces.IServices;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Infrastructure.Storage
{
    public sealed class MinioFileStorage : IFileStorage
    {
        private readonly IMinioClient _client;
        private readonly string _publicBaseUrl;

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

        public async Task<string> UploadAsync(string bucket, string objectName, byte[] content, string contentType, CancellationToken cancellationToken = default)
        {
            var exists = await _client.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(bucket), cancellationToken);

            if (!exists)
            {
                await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket), cancellationToken);
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
    }
}

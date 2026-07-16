namespace AiFashionStudio.Platform.Infrastructure.Storage
{
    public class MinioSettings
    {
        public const string SectionName = "Minio";

        public string Endpoint { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string PublicBaseUrl { get; set; } = string.Empty;
        public bool UseSSL { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using System.Diagnostics;

namespace CloudAssignment.Controllers
{
    public class FileController : ControllerBase
    {
        private readonly IAmazonS3 _s3Client;
        public FileController()
        {
            var credentials = new BasicAWSCredentials("Secret Key", "Private key");
            _s3Client = new AmazonS3Client(credentials, Amazon.RegionEndpoint.USWest1);
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllFilesAsync(string bucketName, string? prefix)
        {
            var bucketExists = await _s3Client.DoesS3BucketExistAsync(bucketName);
            if (!bucketExists) return NotFound($"Bucket {bucketName} does not exist.");
            var request = new ListObjectsV2Request()
            {
                BucketName = bucketName,
                Prefix = prefix
            };
            var result = await _s3Client.ListObjectsV2Async(request);
            var s3Objects = result.S3Objects.Select(s =>
            {
                var urlRequest = new GetPreSignedUrlRequest()
                {
                    BucketName = bucketName,
                    Key = s.Key,
                    Expires = DateTime.UtcNow.AddMinutes(1)
                };
                return new S3ObjectDto()
                {
                    Name = s.Key.ToString(),
                    PresignedUrl = _s3Client.GetPreSignedURL(urlRequest),
                };
            });
            return Ok(s3Objects);
        }

            [HttpGet("get-by-key")]
            public async Task<IActionResult> GetFileByKeyAsync(string bucketName, string key)
            {
                var bucketExists = await _s3Client.DoesS3BucketExistAsync(bucketName);
                if (!bucketExists) return NotFound($"Bucket {bucketName} does not exist.");
                var s3Object = await _s3Client.GetObjectAsync(bucketName, key);
                return File(s3Object.ResponseStream, s3Object.Headers.ContentType);
            }
     }
}

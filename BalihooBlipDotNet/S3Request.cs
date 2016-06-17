using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BalihooBlipDotNet
{
    internal static class S3Request
    {
        /// <summary>
        /// Upload a bulk location file to S3.
        /// </summary>
        /// <param name="blipRequest">A credentialed BlipRequest object.</param>
        /// <param name="brandKey">The unique identifier for a single brand.</param>
        /// <param name="filePath">The path to the file to be uploaded.</param>
        /// <returns>BlipResponse object with a status code and body text if applicable.</returns>
        internal static BlipResponse Upload(BlipRequest blipRequest, string brandKey, string filePath)
        {
            // Stream file contents
            byte[] byteStream;
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                byteStream = new byte[fileStream.Length];
                fileStream.Read(byteStream, 0, (int)fileStream.Length);
            }

            // Compress file contents
            var compressed = new MemoryStream();
            using (var gzip = new GZipStream(compressed, CompressionMode.Compress, false))
            {
                gzip.Write(byteStream, 0, byteStream.Length);
            }

            // Get authorization to upload file from BLIP
            var fileMd5 = GenerateMd5Hash(compressed.ToArray());
            var path = $"/brand/{brandKey}/authorizeUpload?fileMD5={fileMd5}";
            var authResponse = blipRequest.ExecuteCommand(BlipRequest.Command.Get, path).Result;

            if (authResponse.StatusCode != 200) return authResponse; // Return error response if auth fails

            dynamic auth = JsonConvert.DeserializeObject(authResponse.Body);
            var formData = auth.data.ToString();
            var s3Bucket = auth.s3Bucket.ToString();
            var s3Key = auth.data.key.ToString();
            var s3Path = $"s3://{s3Bucket}/{s3Key}";
            const string mimeType = "text/plain";

            var uploadResponse = PostFile(s3Bucket, formData, mimeType, compressed.ToArray());

            // If upload fails Return error response else return success response
            return uploadResponse.StatusCode != 204 ? uploadResponse : new BlipResponse(uploadResponse.StatusCode, s3Path);
        }

        /// <summary>
        /// Create Multipart form-data HTTP Post to upload file to S3.
        /// </summary>
        /// <param name="s3Bucket">The Amazon S3 bucket name.</param>
        /// <param name="formData">The JSON object containing the pre-signed URL data.</param>
        /// <param name="mimeType">The MIME type of the file to be uploaded.</param>
        /// <param name="compressedFile">The gzipped file contents.</param>
        /// <returns>BlipResponse object with a status code and body text if applicable.</returns>
        private static BlipResponse PostFile(string s3Bucket, string formData, string mimeType, byte[] compressedFile)
        {
            HttpResponseMessage response;
            dynamic data = JsonConvert.DeserializeObject(formData);

            using (var client = new HttpClient())
            {
                var url = $"https://s3.amazonaws.com/{s3Bucket}";
                var requestContent = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(compressedFile);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
                
                // Special parsing required due to a hyphen in the key
                string contentMd5 = JObject.Parse(data.ToString())["content-md5"];

                requestContent.Add(new StringContent(data.acl.ToString()), "acl");
                requestContent.Add(new StringContent(data.bucket.ToString()), "bucket");
                requestContent.Add(new StringContent(data.key.ToString()), "key");
                requestContent.Add(new StringContent(contentMd5), "content-md5");
                requestContent.Add(new StringContent(data.policy.ToString()), "policy");
                requestContent.Add(new StringContent(data.signature.ToString()), "signature");
                requestContent.Add(new StringContent(data.AWSAccessKeyId.ToString()), "AWSAccessKeyId");
                requestContent.Add(new StringContent(mimeType), "content-type");
                requestContent.Add(fileContent, "file"); // The file must be added last
                
                response = client.PostAsync(url, requestContent).Result;
            }

            var statusCode = (int)response.StatusCode;

            // If the upload is not successful return the error message from S3 else return success response
            return statusCode != 204 ? new BlipResponse(statusCode, response.Content.ToString()) : new BlipResponse(statusCode, "");
        }

        /// <summary>
        /// Generate an MD5 checksum.
        /// </summary>
        /// <param name="content">The content to evaluate.</param>
        /// <returns>The checksum as a string.</returns>
        private static string GenerateMd5Hash(byte[] content)
        {
            var sb = new StringBuilder();
            var fileMd5 = MD5.Create();
            var hash = fileMd5.ComputeHash(content);

            foreach (var hashByte in hash)
            {
                sb.Append(hashByte.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}

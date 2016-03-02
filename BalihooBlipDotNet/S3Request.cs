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
    internal class S3Request
    {
        internal BlipResponse Upload(BlipRequest blipRequest, string brandKey, string filePath)
        {
            // Compress file contents
            var byteStream = File.ReadAllBytes(filePath);
            using (var fs = new FileStream(filePath, FileMode.CreateNew))
            using (var gzip = new GZipStream(fs, CompressionMode.Compress, false))
            {
                gzip.Write(byteStream, 0, byteStream.Length);
            }

            // Get authorization to upload file from BLIP
            var fileMd5 = GenerateMd5Hash(byteStream);
            var path = $"/brand/{brandKey}/authorizeUpload?fileMD5={fileMd5}";
            var authResponse = blipRequest.ExecuteCommand(BlipRequest.Command.Get, path).Result;

            if (authResponse.StatusCode != 200) return authResponse; // Return error response if auth fails

            var mimeType = "text/plain"; // TODO: Inspect file for MIME type
            dynamic auth = JsonConvert.DeserializeObject(authResponse.Body);
            string formData = auth.data;
            string s3Bucket = auth.s3Bucket;
            string s3Key = auth.data.key;
            var s3Path = $"s3://{s3Bucket}/{s3Key}";

            var uploadResponse = PostFile(s3Bucket, formData, mimeType, byteStream);

            // If upload fails Return error response else return success response
            return uploadResponse.StatusCode != 204 ? uploadResponse : new BlipResponse(uploadResponse.StatusCode, s3Path);
        }

        private static BlipResponse PostFile(string s3Bucket, string formData, string mimeType, byte[] compressedFile)
        {
            HttpResponseMessage response;
            dynamic data = JsonConvert.DeserializeObject(formData);

            using (var client = new HttpClient())
            {
                var requestContent = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(compressedFile);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);

                requestContent.Add(new StringContent(data.acl), "acl");
                requestContent.Add(new StringContent(data.bucket), "bucket");
                requestContent.Add(new StringContent(data.key), "key");
                requestContent.Add(new StringContent(JObject.Parse(data)["conent-md5"]), "content-md5");
                requestContent.Add(new StringContent(data.policy), "policy");
                requestContent.Add(new StringContent(data.signature), "signature");
                requestContent.Add(new StringContent(data.AWSAccessKeyId), "AWSAccessKeyId");
                requestContent.Add(new StringContent(mimeType), "content-type");
                requestContent.Add(fileContent, "file"); // The file must be added last

                var url = $"https://s3.amazonaws.com/{s3Bucket}";
                response = client.PostAsync(url, requestContent).Result;
            }

            var statusCode = (int)response.StatusCode;

            // If the upload is not successful return the error message from S3 else return success response
            return statusCode != 204 ? new BlipResponse(statusCode, response.Content.ToString()) : new BlipResponse(200, "");
        }

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

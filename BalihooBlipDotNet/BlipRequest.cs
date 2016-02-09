using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BalihooBlipDotNet
{
    /// <summary>
    /// An HTTP request to the BLIP API.
    /// </summary>
    internal class BlipRequest
    {
        private string Credentials { get; }
        private string Endpoint { get; }

        internal enum Command
        {
            Get,
            Put,
            Post,
            Delete
        }

        /// <summary>
        /// The BlipRequest constructor.
        /// </summary>
        /// <param name="credentials">The Base64-encoded BLIP credentials.</param>
        /// <param name="endpoint">The base URL for the BLIP environment.</param>
        internal BlipRequest(string credentials, string endpoint)
        {
            Credentials = credentials;
            Endpoint = endpoint;
        }

        /// <summary>
        /// Executes the specified HTTP command.
        /// </summary>
        /// <param name="command">The HTTP command.</param>
        /// <param name="path">The URI path for the API function to be executed.</param>
        /// <param name="content">Any content that may need to be supplied to the API. Defaults to null.</param>
        /// <returns>A BlipResponse object.</returns>
        internal async Task<BlipResponse> ExecuteCommand(Command command, string path, string content=null)
        {
            var encodedPath = Uri.EscapeUriString(path);

            using (var httpClient = new HttpClient())
            using (var client = ConfigureClient(httpClient))
            {
                switch (command)
                {
                    case Command.Get:
                        {
                            using (var response = await client.GetAsync(encodedPath))
                            {
                                response.EnsureSuccessStatusCode();
                                return await BuildResponse(response);
                            }
                        }
                    case Command.Put:
                        {
                            using (var httpContent = new StringContent(content, Encoding.UTF8, "application/json"))
                            using (var response = await client.PutAsync(encodedPath, httpContent))
                            {
                                response.EnsureSuccessStatusCode();
                                return await BuildResponse(response);
                            }
                        }
                    case Command.Post:
                        {
                            using (var httpContent = new StringContent(content, Encoding.UTF8, "application/json"))
                            using (var response = await client.PostAsync(encodedPath, httpContent))
                            {
                                response.EnsureSuccessStatusCode();
                                return await BuildResponse(response);
                            }
                        }
                    case Command.Delete:
                        {
                            using (var response = await client.DeleteAsync(encodedPath))
                            {
                                response.EnsureSuccessStatusCode();
                                return await BuildResponse(response);
                            }
                        }
                    default:
                        throw new ArgumentOutOfRangeException($"Invalid HTTP Command: {command}");
                }
            }
        }

        /// <summary>
        /// Configures the HTTP client with credentials, headers, etc.
        /// </summary>
        /// <param name="client">The HTTPClient to configure.</param>
        /// <returns>A configured HTTPClient.</returns>
        private HttpClient ConfigureClient(HttpClient client)
        {
            client.BaseAddress = new Uri(Endpoint);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Credentials);
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            return client;
        }

        /// <summary>
        /// Converts an HTTPResponseMessage into a BlipResponse object.
        /// </summary>
        /// <param name="response">The HTTPResponseMessage object to convert.</param>
        /// <returns>A BlipResponse object.</returns>
        private async Task<BlipResponse> BuildResponse(HttpResponseMessage response)
        {
            var statusCode = (int)response.StatusCode;
            var body = await response.Content.ReadAsStringAsync();

            return new BlipResponse(statusCode, body);
        }
    }
}

using System;
using System.Linq;
using System.Text;

namespace BalihooBlipDotNet
{
    /// <summary>
    /// The BLIP object and its methods.
    /// </summary>
    public class Blip
    {
        private string Credentials { get; }
        private string Endpoint { get; }

        /// <summary>
        /// The Blip constructor.
        /// </summary>
        /// <param name="apiKey">The key used to access the BLIP API.</param>
        /// <param name="secretKey">The secret key used to access the BLIP API.</param>
        /// <param name="endpoint">The BLIP endpoint to target. Defaults to Balihoo's production environment.</param>
        public Blip(string apiKey, string secretKey, string endpoint="https://blip.balihoo-cloud.com")
        {
            Credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{apiKey}:{secretKey}"));
            Endpoint = endpoint.TrimEnd('/');
        }

        /// <summary>
        /// Ping the BLIP API.
        /// </summary>
        /// <returns>BlipResponse object with a status code and body text if applicable.</returns>
        public BlipResponse Ping()
        {
            var request = new BlipRequest(Credentials, Endpoint);

            return request.ExecuteCommand(BlipRequest.Command.Get, "/ping").Result;
        }

        /// <summary>
        /// Get a list of brandKeys that the API user is authorized to access.
        /// </summary>
        /// <returns>BlipResponse object with a status code and body text if applicable.</returns>
        public BlipResponse GetBrandKeys()
        {
            var request = new BlipRequest(Credentials, Endpoint);

            return request.ExecuteCommand(BlipRequest.Command.Get, "/brand").Result;
        }

        /// <summary>
        /// Get a list of data sources available for an individual brand.
        /// </summary>
        /// <param name="brandKey">The unique identifier for a single brand.</param>
        /// <returns>BlipResponse object with a status code and body text if applicable.</returns>
        public BlipResponse GetBrandSources(string brandKey)
        {
            var path = $"/brand/{brandKey}/source";
            var request = new BlipRequest(Credentials, Endpoint);

            return request.ExecuteCommand(BlipRequest.Command.Get, path).Result;
        }

        /// <summary>
        /// Get a list of data projections available for an individual brand.
        /// </summary>
        /// <param name="brandKey">The unique identifier for a single brand.</param>
        /// <returns>BlipResponse object with a status code and body text if applicable.</returns>
        public BlipResponse GetBrandProjections(string brandKey)
        {
            var path = $"/brand/{brandKey}/projection";
            var request = new BlipRequest(Credentials, Endpoint);

            return request.ExecuteCommand(BlipRequest.Command.Get, path).Result;
        }

        /// <summary>
        /// Get a list of locationKeys for all locations belonging to the specified brand.
        /// </summary>
        /// <param name="brandKey">The unique identifier for a single brand.</param>
        /// <param name="projection">Optionally filter data to locations in a single projecttion. Defaults to "universal".</param>
        /// <returns>BlipResponse object with a status code and body text if applicable.</returns>
        public BlipResponse GetLocationKeys(string brandKey, string projection="universal")
        {
            var path = $"/brand/{brandKey}/location?projection={projection}";
            var request = new BlipRequest(Credentials, Endpoint);

            return request.ExecuteCommand(BlipRequest.Command.Get, path).Result;
        }

        /// <summary>
        /// Get data for an individual location that belongs to the specified brand.
        /// </summary>
        /// <param name="brandKey">The unique identifier for a single brand.</param>
        /// <param name="locationKey">The unique identifier for a single location within the brand.</param>
        /// <param name="projection">Optionally filter data in a single projection. Defaults to "universal".</param>
        /// <param name="includeRefs">Optionally include objects referenced by the location in its data. Defaults to false.</param>
        /// <returns>BlipResponse object with a status code and body text if applicable.</returns>
        public BlipResponse GetLocation(string brandKey, string locationKey, string projection="universal", bool includeRefs=false)
        {
            var path = $"/brand/{brandKey}/location/{locationKey}?projection={projection}&includeRefs={includeRefs.ToString().ToLower()}";
            var request = new BlipRequest(Credentials, Endpoint);

            return request.ExecuteCommand(BlipRequest.Command.Get, path).Result;
        }

        /// <summary>
        /// Get data for locations in a single brand that match the specified BLIP query.
        /// </summary>
        /// <param name="brandKey">The unique identifier for a single brand.</param>
        /// <param name="query">A stringified JSON query used to filter locations in BLIP.</param>
        /// <param name="view">Optionally specify the view returned. Defaults to "full".</param>
        /// <returns>BlipResponse object with a status code and body text if applicable.</returns>
        public BlipResponse QueryLocations(string brandKey, string query, string view="full")
        {
            var path = $"/brand/{brandKey}/locationList";
            var queryParam = $"{{\"query\": {query}, \"view\": \"{view}\"}}";
            var request = new BlipRequest(Credentials, Endpoint);

            return request.ExecuteCommand(BlipRequest.Command.Post, path, queryParam).Result;
        }

        /// <summary>
        /// Add or update a location.
        /// </summary>
        /// <param name="brandKey">The unique identifier for a single brand.</param>
        /// <param name="locationKey">The unique identifier for a single location within the brand.</param>
        /// <param name="source">The unique identifier for the data source being used to add/update the location.</param>
        /// <param name="locationData">The stringified JSON location document.</param>
        /// <returns>BlipResponse object with a status code and body text if applicable.</returns>
        public BlipResponse PutLocation(string brandKey, string locationKey, string source, string locationData)
        {
            var path = $"/brand/{brandKey}/location/{locationKey}?source={source}";
            var request = new BlipRequest(Credentials, Endpoint);

            return request.ExecuteCommand(BlipRequest.Command.Put, path, locationData).Result;
        }

        /// <summary>
        /// Delete a location.
        /// </summary>
        /// <param name="brandKey">The unique identifier for a single brand.</param>
        /// <param name="locationKey">The unique identifier for a single location within the brand.</param>
        /// <param name="source">The unique identifier for the data source being used to delete the location.</param>
        /// <returns>BlipResponse object with a status code and body text if applicable.</returns>
        public BlipResponse DeleteLocation(string brandKey, string locationKey, string source)
        {
            var path = $"/brand/{brandKey}/location/{locationKey}?source={source}";
            var request = new BlipRequest(Credentials, Endpoint);

            return request.ExecuteCommand(BlipRequest.Command.Delete, path).Result;
        }

        public BlipResponse BulkLoad(string brandKey, string source, string filePath, bool implicitDelete,
            int expectedRecordCount, string successEmail = null, string failEmail = null,
            string successCallbackUrl = null, string failCallbackUrl = null)
        {
            // Use pre-seigned auth from BLIP to upload the file to S3
            var blipRequest = new BlipRequest(Credentials, Endpoint);
            var s3UploadResponse = new S3Request().Upload(blipRequest, brandKey, filePath);

            if (s3UploadResponse.StatusCode != 204) return s3UploadResponse;

            var path = $"/brand/{brandKey}/bulkLoad";
            var s3Path = s3UploadResponse.Body;
            path += $"s3Path={s3Path}&source={source}&implicitDelete={implicitDelete}&expectedRecordCount={expectedRecordCount}";

            // Validate and add optional params
            if (!string.IsNullOrEmpty(successEmail))
            {
                if (IsValidEmail(successEmail))
                    path += $"&successEmail={successEmail}";
                else
                    return new BlipResponse(400, $"Error: successEmail is not valid. {successEmail}");

                if (IsValidEmail(failEmail))
                    path += $"&failEmail={failEmail}";
                else
                    return new BlipResponse(400, $"Error: failEmail is not valid. {failEmail}");

                if (IsValidUrl(successCallbackUrl))
                    path += $"&successCallbackUrl={successCallbackUrl}";
                else
                    return new BlipResponse(400, $"Error: successCallbackUrl is not valid. {successCallbackUrl}");

                if (IsValidUrl(failCallbackUrl))
                    path += $"&failCallbackUrl={failCallbackUrl}";
                else
                    return new BlipResponse(400, $"Error: failCallbackUrl is not valid. {failCallbackUrl}");
            }
            return blipRequest.ExecuteCommand(BlipRequest.Command.Post, path).Result;
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;
            try
            {
                // Return false if any email address in the list is invalid. Otherwise return true.
                return !(from emailString in email.Split(',')
                         let addr = new System.Net.Mail.MailAddress(emailString)
                         where addr.Address != emailString
                         select emailString).Any();
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidUrl(string url)
        {
            Uri uriResult;
            return Uri.TryCreate(url, UriKind.Absolute, out uriResult) 
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}

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
        /// <param name="pageSize">Optionally specify the number of results to include in each page of results.</param>
        /// <param name="pageNumber">Optionally specify the page index to return.</param>
        /// <param name="sortColumn">The column by which to sort results. ('name' or 'locationKey' -- defaults to 'locationKey').</param>
        /// <param name="sortDirection">The direction to sort results. ('asc' or 'desc' -- defaults to 'asc').</param>
        /// <returns>BlipResponse object with a status code and body text if applicable.</returns>
        public BlipResponse QueryLocations(string brandKey, string query, string view="full",
            int pageSize=0, int pageNumber=0, string sortColumn="locationKey", string sortDirection="asc")
        {
            var path = $"/brand/{brandKey}/locationList";
            var queryParam = $"{{\"query\":{query},\"view\":\"{view}\"";

            if (pageSize > 0)
                queryParam += $",\"pageSize\":{pageSize},\"pageNumber\":{pageNumber}";

            queryParam += $",\"sortColumn\":\"{sortColumn}\",\"sortDirection\":\"{sortDirection}\"}}";
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

        /// <summary>
        /// Load a bulk location file into BLIP.
        /// </summary>
        /// <param name="brandKey">The unique identifier for a single brand.</param>
        /// <param name="source">The unique identifier for the data source.</param>
        /// <param name="filePath">The full path to the bulk location file.</param>
        /// <param name="implicitDelete">Whether or not to delete locations from BLIP if they're missing from the file.</param>
        /// <param name="expectedRecordCount">The number of location records to expect in the file.</param>
        /// <param name="successEmail">An optional email address to notify upon success. Can be a comma-delimited list.</param>
        /// <param name="failEmail">An optional email address to notify upon failure. Can be a comma-delimited list.</param>
        /// <param name="successCallbackUrl">An optional URL to call upon success.</param>
        /// <param name="failCallbackUrl">An optional URL to call upon failure.</param>
        /// <returns>BlipResponse object with a status code and body text if applicable.</returns>
        public BlipResponse BulkLoad(string brandKey, string source, string filePath, bool implicitDelete,
            int expectedRecordCount, string successEmail = null, string failEmail = null,
            string successCallbackUrl = null, string failCallbackUrl = null)
        {
            // Validate optional parameters
            var optionalParams = "";
            if (!string.IsNullOrEmpty(successEmail))
            {
                if (IsValidEmail(successEmail))
                    optionalParams += $"&successEmail={successEmail}";
                else
                    return new BlipResponse(400, $"Error: successEmail is not valid. {successEmail}");
            }
            if (!string.IsNullOrEmpty(failEmail))
            {
                if (IsValidEmail(failEmail))
                    optionalParams += $"&failEmail={failEmail}";
                else
                    return new BlipResponse(400, $"Error: failEmail is not valid. {failEmail}");
            }
            if (!string.IsNullOrEmpty(successCallbackUrl))
            {
                if (IsValidUrl(successCallbackUrl))
                    optionalParams += $"&successCallback={successCallbackUrl}";
                else
                    return new BlipResponse(400, $"Error: successCallbackUrl is not valid. {successCallbackUrl}");
            }
            if (!string.IsNullOrEmpty(failCallbackUrl))
            {
                if (IsValidUrl(failCallbackUrl))
                    optionalParams += $"&failCallback={failCallbackUrl}";
                else
                    return new BlipResponse(400, $"Error: failCallbackUrl is not valid. {failCallbackUrl}");
            }

            // Use pre-seigned auth from BLIP to upload the file to S3
            var blipRequest = new BlipRequest(Credentials, Endpoint);
            var s3UploadResponse = S3Request.Upload(blipRequest, brandKey, filePath);

            // Return error response if S3 upload fails
            if (s3UploadResponse.StatusCode != 204)
            {
                return s3UploadResponse;
            }

            // Build query string
            var path = $"/brand/{brandKey}/bulkLoad?";
            var s3Path = s3UploadResponse.Body;
            var delete = implicitDelete.ToString().ToLower();
            var count = expectedRecordCount.ToString();
            path += $"s3Path={s3Path}&source={source}&implicitDelete={delete}&expectedRecordCount={count}";
            if (!string.IsNullOrEmpty(optionalParams)) { path += optionalParams; }

            // Ask BLIP to load the file from S3 and return its response (success of failure)
            return blipRequest.ExecuteCommand(BlipRequest.Command.Get, path).Result;
        }

        /// <summary>
        /// Validate email address or comma delimited list of email addresses.
        /// </summary>
        /// <param name="email">The email address(es) to validate.</param>
        /// <returns>Boolean validation response.</returns>
        private static bool IsValidEmail(string email)
        {
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

        /// <summary>
        /// Validate URL.
        /// </summary>
        /// <param name="url">The URL to validate.</param>
        /// <returns>Boolean validation response.</returns>
        private static bool IsValidUrl(string url)
        {
            try
            {
                Uri uriResult;
                return Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            }
            catch
            {
                return false;
            }
        }
    }
}

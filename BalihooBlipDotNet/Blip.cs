using System;
using System.Text;
using System.Threading.Tasks;

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
        public Task<BlipResponse> Ping()
        {
            var request = new BlipRequest(Credentials, Endpoint);

            return request.ExecuteCommand(BlipRequest.Command.Get, "/ping");
        }

        /// <summary>
        /// Get a list of brandKeys that the API user is authorized to access.
        /// </summary>
        /// <returns>BlipResponse object with a status code and body text if applicable.</returns>
        public Task<BlipResponse> GetBrandKeys()
        {
            var request = new BlipRequest(Credentials, Endpoint);

            return request.ExecuteCommand(BlipRequest.Command.Get, "/brand");
        }

        /// <summary>
        /// Get a list of data sources available for an individual brand.
        /// </summary>
        /// <param name="brandKey">The unique identifier for a single brand.</param>
        /// <returns>BlipResponse object with a status code and body text if applicable.</returns>
        public Task<BlipResponse> GetBrandSources(string brandKey)
        {
            var path = $"/brand/{brandKey}/source";
            var request = new BlipRequest(Credentials, Endpoint);

            return request.ExecuteCommand(BlipRequest.Command.Get, path);
        }

        /// <summary>
        /// Get a list of data projections available for an individual brand.
        /// </summary>
        /// <param name="brandKey">The unique identifier for a single brand.</param>
        /// <returns>BlipResponse object with a status code and body text if applicable.</returns>
        public Task<BlipResponse> GetBrandProjections(string brandKey)
        {
            var path = $"/brand/{brandKey}/projection";
            var request = new BlipRequest(Credentials, Endpoint);

            return request.ExecuteCommand(BlipRequest.Command.Get, path);
        }

        /// <summary>
        /// Get a list of locationKeys for all locations belonging to the specified brand.
        /// </summary>
        /// <param name="brandKey">The unique identifier for a single brand.</param>
        /// <param name="projection">Optionally filter data to locations in a single projecttion. Defaults to "universal".</param>
        /// <returns>BlipResponse object with a status code and body text if applicable.</returns>
        public Task<BlipResponse> GetLocationKeys(string brandKey, string projection="universal")
        {
            var path = $"/brand/{brandKey}/location?projection={projection}";
            var request = new BlipRequest(Credentials, Endpoint);

            return request.ExecuteCommand(BlipRequest.Command.Get, path);
        }

        /// <summary>
        /// Get data for an individual location that belongs to the specified brand.
        /// </summary>
        /// <param name="brandKey">The unique identifier for a single brand.</param>
        /// <param name="locationKey">The unique identifier for a single location within the brand.</param>
        /// <param name="projection">Optionally filter data in a single projection. Defaults to "universal".</param>
        /// <param name="includeRefs">Optionally include objects referenced by the location in its data. Defaults to false.</param>
        /// <returns>BlipResponse object with a status code and body text if applicable.</returns>
        public Task<BlipResponse> GetLocation(string brandKey, string locationKey, string projection="universal", bool includeRefs=false)
        {
            var path = $"/brand/{brandKey}/location/{locationKey}?projection={projection}&includeRefs={includeRefs.ToString().ToLower()}";
            var request = new BlipRequest(Credentials, Endpoint);

            return request.ExecuteCommand(BlipRequest.Command.Get, path);
        }

        /// <summary>
        /// Get data for locations in a single brand that match the specified BLIP query.
        /// </summary>
        /// <param name="brandKey">The unique identifier for a single brand.</param>
        /// <param name="query">A stringified JSON query used to filter locations in BLIP.</param>
        /// <param name="view">Optionally specify the view returned. Defaults to "full".</param>
        /// <returns>BlipResponse object with a status code and body text if applicable.</returns>
        public Task<BlipResponse> QueryLocations(string brandKey, string query, string view="full")
        {
            var path = $"/brand/{brandKey}/locationList";
            var queryParam = $"{{\"query\": {query}, \"view\": \"{view}\"}}";
            var request = new BlipRequest(Credentials, Endpoint);

            return request.ExecuteCommand(BlipRequest.Command.Post, path, queryParam);
        }

        /// <summary>
        /// Add or update a location.
        /// </summary>
        /// <param name="brandKey">The unique identifier for a single brand.</param>
        /// <param name="locationKey">The unique identifier for a single location within the brand.</param>
        /// <param name="source">The unique identifier for the data source being used to add/update the location.</param>
        /// <param name="locationData">The stringified JSON location document.</param>
        /// <returns>BlipResponse object with a status code and body text if applicable.</returns>
        public Task<BlipResponse> PutLocation(string brandKey, string locationKey, string source, string locationData)
        {
            var path = $"/brand/{brandKey}/location/{locationKey}?source={source}";
            var request = new BlipRequest(Credentials, Endpoint);

            return request.ExecuteCommand(BlipRequest.Command.Put, path, locationData);
        }

        /// <summary>
        /// Delete a location.
        /// </summary>
        /// <param name="brandKey">The unique identifier for a single brand.</param>
        /// <param name="locationKey">The unique identifier for a single location within the brand.</param>
        /// <param name="source">The unique identifier for the data source being used to delete the location.</param>
        /// <returns>BlipResponse object with a status code and body text if applicable.</returns>
        public Task<BlipResponse> DeleteLocation(string brandKey, string locationKey, string source)
        {
            var path = $"/brand/{brandKey}/location/{locationKey}?source={source}";
            var request = new BlipRequest(Credentials, Endpoint);

            return request.ExecuteCommand(BlipRequest.Command.Delete, path);
        }
    }
}

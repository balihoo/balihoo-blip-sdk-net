namespace BalihooBlipDotNet
{
    /// <summary>
    /// The response returned when a BLIP API call is made.
    /// </summary>
    public class BlipResponse
    {
        public int StatusCode { get; }
        public string Body { get; }

        /// <summary>
        /// The BlipResponse constructor.
        /// </summary>
        /// <param name="statusCode">The numeric HTTP status code that is returned.</param>
        /// <param name="body">The body text that is returned in the HTTP response.</param>
        public BlipResponse(int statusCode, string body)
        {
            StatusCode = statusCode;
            Body = body;
        }
    }
}

using System;

namespace ProcessingEngineDemo.Common
{
    /// <summary>
    /// The token used to access the Spotify web API.
    /// </summary>
    public class SpotifyToken
    {
        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the type of the token.
        /// </summary>
        public string TokenType { get; set; }

        /// <summary>
        /// Gets or sets the time the token will expire.
        /// </summary>
        public DateTime Expiration { get; set; }
    }
}

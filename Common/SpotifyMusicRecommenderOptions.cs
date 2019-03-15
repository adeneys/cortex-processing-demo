namespace ProcessingEngineDemo.Common
{
    /// <summary>
    /// Options for the <see cref="SpotifyMusicRecommender"/> class.
    /// </summary>
    public class SpotifyMusicRecommenderOptions
    {
        /// <summary>
        /// Gets or sets the Client ID used in calls to the Spotify API.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the Client Secret used in calls to the Spotify API.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the base URL for spotify web APIs.
        /// </summary>
        public string BaseServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets the base URL for spotify account web APIs.
        /// </summary>
        public string BaseAccountsUrl { get; set; }
    }
}

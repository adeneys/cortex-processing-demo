using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace ProcessingEngineDemo.Common
{
    /// <summary>
    /// An implementation of the <see cref="IMusicRecommender"/> interface which uses the Spotify API to make the recommendations.
    /// </summary>
    public class SpotifyMusicRecommender : IMusicRecommender
    {
        private readonly SpotifyMusicRecommenderOptions _options = null;
        private SpotifyToken _spotifyToken = null;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="options">The options containig the settings for the service.</param>
        public SpotifyMusicRecommender(IOptions<SpotifyMusicRecommenderOptions> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _options = options.Value;
        }

        /// <summary>
        /// Provides a recommendation for a number of albums, based on a list of artists of other albums.
        /// </summary>
        /// <param name="artistIds">The IDs of the artists to base the recommendation on.</param>
        /// <param name="recommendationLimit">The maximum number of recommendations to make.</param>
        /// <returns>A task yielding a number of albums.</returns>
        public async Task<IEnumerable<AlbumDescriptor>> GetRecommendationsAsync(IEnumerable<string> artistIds, int recommendationLimit)
        {
            await EnsureSpotifyTokenAsync();

            var client = new HttpClient
            {
                BaseAddress = new Uri(_options.BaseServiceUrl)
            };

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_spotifyToken.TokenType, _spotifyToken.AccessToken);

            var queryString = HttpUtility.ParseQueryString("");
            queryString["limit"] = recommendationLimit.ToString();
            queryString["market"] = "AU";
            queryString["seed_artists"] = string.Join(",", artistIds);// "6Ghvu1VvMGScGpOUJBAHNH,2VYQTNDsvvKN9wmU5W7xpj";

            var response = await client.GetAsync("/v1/recommendations?" + queryString);
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JObject.Parse(responseContent);

            var tracks = responseObject["tracks"].ToObject<List<JObject>>();
            var returnAlbums = new List<AlbumDescriptor>();

            foreach (var track in tracks)
            {
                var artist = track["artists"].First;
                var album = track["album"];

                returnAlbums.Add(
                    new AlbumDescriptor
                    {
                        ArtistId = artist["id"].ToString(),
                        ArtistName = artist["name"].ToString(),
                        AlbumId = album["id"].ToString(),
                        AlbumName = album["name"].ToString()
                    }
                );
            }

            return returnAlbums;
        }

        /// <summary>
        /// Ensures we have a valid and unexpired token for accessing the spotify API. Calls the Spotify API as necessary to create new tokens.
        /// </summary>
        /// <returns>A task indicating the status of the call.</returns>
        private async Task EnsureSpotifyTokenAsync()
        {
            if (_spotifyToken != null && _spotifyToken.Expiration > DateTime.Now.AddSeconds(60))
                return;

            var client = new HttpClient
            {
                BaseAddress = new Uri(_options.BaseAccountsUrl)
            };

            var rawHeaderValue = Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}");
            var headerValue = Convert.ToBase64String(rawHeaderValue);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", headerValue);

            var request = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            var response = await client.PostAsync("/api/token", request);
            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenObject = JObject.Parse(responseContent);

            _spotifyToken = new SpotifyToken
            {
                AccessToken = tokenObject["access_token"].ToString(),
                TokenType = tokenObject["token_type"].ToString(),
                Expiration = DateTime.Now.AddSeconds(tokenObject["expires_in"].ToObject<int>())
            };
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProcessingEngineDemo.Common
{
    /// <summary>
    /// Defines the music recommender service, used to recommend albums.
    /// </summary>
    public interface IMusicRecommender
    {
        /// <summary>
        /// Provides a recommendation for a number of albums, based on a list of artists of other albums.
        /// </summary>
        /// <param name="artistIds">The IDs of the artists to base the recommendation on.</param>
        /// <param name="recommendationLimit">The maximum number of recommendations to make.</param>
        /// <returns>A task yielding a number of albums.</returns>
        Task<IEnumerable<AlbumDescriptor>> GetRecommendationsAsync(IEnumerable<string> artistIds, int recommendationLimit);
    }
}

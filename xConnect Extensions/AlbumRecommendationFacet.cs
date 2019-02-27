using System.Collections.Generic;
using ProcessingEngineDemo.Common;
using Sitecore.XConnect;

namespace ProcessingEngineDemo.XConnectExtensions
{
    /// <summary>
    /// A <see cref="Facet"/> which stores recommendations for other albums the contact may like.
    /// </summary>
    public class AlbumRecommendationFacet : Facet
    {
        /// <summary>
        /// Create a new instance.
        /// </summary>
        public AlbumRecommendationFacet()
        {
            AlbumRecommendations = new List<AlbumDescriptor>();
        }

        /// <summary>
        /// The default name of the facet.
        /// </summary>
        public static string DefaultFacetName = "AlbumRecommendationFacet";

        /// <summary>
        /// Gets or sets the albums that have been recommended.
        /// </summary>
        public List<AlbumDescriptor> AlbumRecommendations { get; set; }
    }
}

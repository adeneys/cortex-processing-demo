using Sitecore.XConnect;
using Sitecore.XConnect.Collection.Model;
using Sitecore.XConnect.Schema;

namespace ProcessingEngineDemo.XConnectExtensions
{
    /// <summary>
    /// A custom xConnect model for music purchases.
    /// </summary>
    public class MusicModel
    {
        /// <summary>
        /// The model instance.
        /// </summary>
        private static XdbModel s_model;

        /// <summary>
        /// Gets the <see cref="XdbModel"/> representing the music model.
        /// </summary>
        public static XdbModel Model => s_model ?? (s_model = InitModel());

        /// <summary>
        /// Initializes the model.
        /// </summary>
        /// <returns>An instance of the model.</returns>
        private static XdbModel InitModel()
        {
            var builder = new XdbModelBuilder("music", new XdbModelVersion(1, 0));

            // Reference the default collection model
            builder.ReferenceModel(CollectionModel.Model);

            // Register events
            builder.DefineEventType<MusicPurchaseOutcome>(true);

            // Register contact facets
            builder.DefineFacet<Contact, AlbumRecommendationFacet>(AlbumRecommendationFacet.DefaultFacetName);

            return builder.BuildModel();
        }
    }
}

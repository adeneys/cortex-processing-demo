using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProcessingEngineDemo.Common;
using ProcessingEngineDemo.XConnectExtensions;
using Sitecore.Processing.Engine.Abstractions;
using Sitecore.Processing.Engine.Storage.Abstractions;
using Sitecore.XConnect;

namespace ProcessingEngineDemo.ProcessingEngineExtensions
{
    /// <summary>
    /// A custom worker used to store the album recommendations into the contact facet.
    /// </summary>
    public class RecommendationFacetStorageWorker : IDeferredWorker
    {
        /// <summary>
        /// The name of the options key containing the name of the table holding the recommendation data.
        /// </summary>
        public const string OptionTableName = "tableName";

        /// <summary>
        /// The name of the options key containing the name of the schema where the table exists.
        /// </summary>
        public const string OptionSchemaName = "schemaName";

        /// <summary>
        /// The name of the table to project the data into.
        /// </summary>
        private readonly string _tableName = null;

        /// <summary>
        /// The table store service used to access the data tables.
        /// </summary>
        private readonly ITableStore _tableStore = null;

        /// <summary>
        /// The <see cref="IXdbContext"/> used to access the xConnect server.
        /// </summary>
        private readonly IXdbContext _xdbContext;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="tableStoreFactory">The <see cref="ITableStoreFactory"/> used to create the <see cref="ITableStore"/> to access the table data.</param>
        /// <param name="xdbContext">The <see cref="IXdbContext"/> used to access the xConnect server.</param>
        /// <param name="options">The options used to configure the worker.</param>
        public RecommendationFacetStorageWorker(
            ITableStoreFactory tableStoreFactory,
            IXdbContext xdbContext,
            IReadOnlyDictionary<string, string> options)
        {
            _tableName = options[OptionTableName];
            var schemaName = options[OptionSchemaName];

            _tableStore = tableStoreFactory.Create(schemaName);

            _xdbContext = xdbContext;
        }

        public void Dispose()
        {
            _tableStore.Dispose();
        }

        public async Task RunAsync(CancellationToken token)
        {
            // Retrieve the recommendation data from the table.
            var rows = await _tableStore.GetRowsAsync(_tableName, CancellationToken.None);

            // Iterate the recommendation data.
            while (await rows.MoveNext())
            {
                foreach (var row in rows.Current)
                {
                    // Row schema
                    // new FieldDefinition("ContactID", FieldKind.Key, FieldDataType.Guid),
                    // new FieldDefinition("AlbumID", FieldKind.Key, FieldDataType.String),
                    // new FieldDefinition("AlbumName", FieldKind.Attribute, FieldDataType.String),
                    // new FieldDefinition("ArtistID", FieldKind.Attribute, FieldDataType.String),
                    // new FieldDefinition("ArtistName", FieldKind.Attribute, FieldDataType.String)

                    var contactId = row.GetGuid(0);
                    var albumId = row.GetString(1);
                    var albumName = row.GetString(2);
                    var artistId = row.GetString(3);
                    var artistName = row.GetString(4);

                    // Load the contact with their album recommendation facet.
                    var contact = await _xdbContext.GetContactAsync(contactId,
                        new ContactExpandOptions(AlbumRecommendationFacet.DefaultFacetName));

                    var facet = contact.GetFacet<AlbumRecommendationFacet>(AlbumRecommendationFacet.DefaultFacetName) ??
                                new AlbumRecommendationFacet();

                    // Add the album to the facet if it doesn't already exist
                    if (facet.AlbumRecommendations.All(x => x.AlbumId != albumId))
                    {
                        facet.AlbumRecommendations.Add(new AlbumDescriptor
                        {
                            AlbumId = albumId,
                            AlbumName = albumName,
                            ArtistId = artistId,
                            ArtistName = artistName
                        });

                        // Update the facet if it's been changed.
                        _xdbContext.SetFacet(contact, AlbumRecommendationFacet.DefaultFacetName, facet);
                        await _xdbContext.SubmitAsync(CancellationToken.None);
                    }
                }
            }
            
            // Delete the table now that we're done with it.
            await _tableStore.RemoveAsync(_tableName, CancellationToken.None);
        }
    }
}

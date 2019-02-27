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
    public class RecommendationFacetStorageWorker : IDeferredWorker
    {
        public const string OptionTableName = "tableName";
        public const string OptionSchemaName = "schemaName";

        private readonly string _tableName = null;
        private readonly ITableStore _tableStore = null;
        private readonly IXdbContext _xdbContext;

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
            var rows = await _tableStore.GetRowsAsync(_tableName, CancellationToken.None);

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

                    var contact = await _xdbContext.GetContactAsync(contactId,
                        new ContactExpandOptions(AlbumRecommendationFacet.DefaultFacetName));

                    var facet = contact.GetFacet<AlbumRecommendationFacet>(AlbumRecommendationFacet.DefaultFacetName) ??
                                new AlbumRecommendationFacet();

                    if (facet.AlbumRecommendations.All(x => x.AlbumId != albumId))
                    {
                        facet.AlbumRecommendations.Add(new AlbumDescriptor
                        {
                            AlbumId = albumId,
                            AlbumName = albumName,
                            ArtistId = artistId,
                            ArtistName = artistName
                        });

                        _xdbContext.SetFacet(contact, AlbumRecommendationFacet.DefaultFacetName, facet);
                        await _xdbContext.SubmitAsync(CancellationToken.None);
                    }
                }
            }
            
            await _tableStore.RemoveAsync(_tableName, CancellationToken.None);
        }
    }
}

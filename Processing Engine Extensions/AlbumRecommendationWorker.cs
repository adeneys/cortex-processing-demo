using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProcessingEngineDemo.Common;
using Sitecore.Processing.Engine.Abstractions;
using Sitecore.Processing.Engine.Projection;
using Sitecore.Processing.Engine.Storage.Abstractions;
using Sitecore.XConnect;

namespace ProcessingEngineDemo.ProcessingEngineExtensions
{
    /// <summary>
    /// A custom worker used to make recommendations for albums.
    /// </summary>
    public class AlbumRecommendationWorker : IDeferredWorker
    {
        /// <summary>
        /// The name of the options key containing the name of the table with the input data.
        /// </summary>
        public const string OptionSourceTableName = "sourceTableName";

        /// <summary>
        /// The name of the options key containing the name of the table to output the recommendations to.
        /// </summary>
        public const string OptionTargetTableName = "targetTableName";

        /// <summary>
        /// The name of the options key containing the name of the schema where the source table exists and the target table should be created.
        /// </summary>
        public const string OptionSchemaName = "schemaName";

        /// <summary>
        /// The name of the options key containing the limit of the number of recommendations to make.
        /// </summary>
        public const string OptionLimit = "limit";

        /// <summary>
        /// The table store service used to access the data tables.
        /// </summary>
        private readonly ITableStore _tableStore = null;

        /// <summary>
        /// The music recommender service used to make the recommendations.
        /// </summary>
        private readonly IMusicRecommender _musicRecommender = null;

        /// <summary>
        /// The name of the table with the input data.
        /// </summary>
        private readonly string _sourceTableName = null;

        /// <summary>
        /// The name of the table to output the recommendations to.
        /// </summary>
        private readonly string _targetTableName = null;

        /// <summary>
        /// The limit of the number of recommendations to make.
        /// </summary>
        private readonly int _limit = 1;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="tableStoreFactory">The <see cref="ITableStoreFactory"/> used to create the <see cref="ITableStore"/> to access the table data.</param>
        /// <param name="musicRecommender">The music recommender service used to make the recommendations.</param>
        /// <param name="options">The options used to configure the worker.</param>
        public AlbumRecommendationWorker(
            ITableStoreFactory tableStoreFactory,
            IMusicRecommender musicRecommender,
            IReadOnlyDictionary<string, string> options)
        {
            _sourceTableName = options[OptionSourceTableName];
            _targetTableName = options[OptionTargetTableName];
            _limit = int.Parse(options[OptionLimit]);

            _musicRecommender = musicRecommender;

            var schemaName = options[OptionSchemaName];
            _tableStore = tableStoreFactory.Create(schemaName);
        }

        public void Dispose()
        {
            _tableStore.Dispose();
        }

        public async Task RunAsync(CancellationToken token)
        {
            // Retrieve the source data from the projection.
            var sourceRows = await _tableStore.GetRowsAsync(_sourceTableName, CancellationToken.None);

            // Define the target table shcema we'll be populating into.
            var targetRows = new List<DataRow>();
            var targetSchema = new RowSchema(
                new FieldDefinition("ContactID", FieldKind.Key, FieldDataType.Guid),
                new FieldDefinition("AlbumID", FieldKind.Key, FieldDataType.String),
                new FieldDefinition("AlbumName", FieldKind.Attribute, FieldDataType.String),
                new FieldDefinition("ArtistID", FieldKind.Attribute, FieldDataType.String),
                new FieldDefinition("ArtistName", FieldKind.Attribute, FieldDataType.String)
            );

            // Iterate the source data.
            while (await sourceRows.MoveNext())
            {
                foreach (var row in sourceRows.Current)
                {
                    // Retrieve the IDs of the album artists from the projected data.
                    var artistIds = row["Artists"].ToString().Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);

                    // Call the recomender service.
                    var recommendations = await _musicRecommender.GetRecommendationsAsync(artistIds, _limit);

                    // Add a target data row for each recommendation.
                    foreach (var album in recommendations)
                    {
                        var targetRow = new DataRow(targetSchema);
                        targetRow.SetGuid(0, row.GetGuid(0));
                        targetRow.SetString(1, album.AlbumId);
                        targetRow.SetString(2, album.AlbumName);
                        targetRow.SetString(3, album.ArtistId);
                        targetRow.SetString(4, album.ArtistName);

                        targetRows.Add(targetRow);
                    }
                }
            }

            // Populate the rows into the target table.
            var tableDefinition = new TableDefinition(_targetTableName, targetSchema);
            var targetTable = new InMemoryTableData(tableDefinition, targetRows);
            await _tableStore.PutTableAsync(targetTable, TimeSpan.FromMinutes(30), CancellationToken.None);
        }
    }
}

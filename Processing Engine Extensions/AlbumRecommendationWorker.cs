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
    public class AlbumRecommendationWorker : IDeferredWorker
    {
        public const string OptionSourceTableName = "sourceTableName";
        public const string OptionTargetTableName = "targetTableName";
        public const string OptionSchemaName = "schemaName";
        public const string OptionLimit = "limit";

        private readonly ITableStore _tableStore = null;
        private readonly IMusicRecommender _musicRecommender = null;
        private readonly string _sourceTableName = null;
        private readonly string _targetTableName = null;
        private readonly int _limit = 1;

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
            var sourceRows = await _tableStore.GetRowsAsync(_sourceTableName, CancellationToken.None);
            var targetRows = new List<DataRow>();
            var targetSchema = new RowSchema(
                new FieldDefinition("ContactID", FieldKind.Key, FieldDataType.Guid),
                new FieldDefinition("AlbumID", FieldKind.Key, FieldDataType.String),
                new FieldDefinition("AlbumName", FieldKind.Attribute, FieldDataType.String),
                new FieldDefinition("ArtistID", FieldKind.Attribute, FieldDataType.String),
                new FieldDefinition("ArtistName", FieldKind.Attribute, FieldDataType.String)
            );

            while (await sourceRows.MoveNext())
            {
                foreach (var row in sourceRows.Current)
                {
                    var artistIds = row["Artists"].ToString().Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                    var recommendations = await _musicRecommender.GetRecommendationsAsync(artistIds, _limit);

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

            var tableDefinition = new TableDefinition(_targetTableName, targetSchema);
            var targetTable = new InMemoryTableData(tableDefinition, targetRows);
            await _tableStore.PutTableAsync(targetTable, TimeSpan.FromMinutes(30), CancellationToken.None);
        }
    }
}

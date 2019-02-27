using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProcessingEngineDemo.XConnectExtensions;
using Sitecore.Processing.Engine.ML.Abstractions;
using Sitecore.Processing.Engine.Projection;
using Sitecore.XConnect;

namespace ProcessingEngineDemo.ProcessingEngineExtensions
{
    public class AlbumRecommendationModel : IModel<Contact>
    {
        public const string OptionTableName = "tableName";

        private readonly string _tableName;

        public AlbumRecommendationModel(IReadOnlyDictionary<string, string> options)
        {
            _tableName = options[OptionTableName];
        }

        public IProjection<Contact> Projection =>
            Sitecore.Processing.Engine.Projection.Projection.Of<Contact>().CreateTabular(
                _tableName,
                contact =>
                    contact.Interactions.Select(interaction =>
                        new
                        {
                            Contact = contact,
                            Artists = interaction.Events.OfType<MusicPurchaseOutcome>().Take(5).Select(outcome =>
                                outcome.Album.ArtistId
                            )
                        }
                    ).Last(),
                cfg => cfg
                    .Key("ContactID", x => x.Contact.Id)
                    .Attribute("Artists", x => string.Join(",", x.Artists))
                // key, attribute, measure
            );

        public Task<ModelStatistics> TrainAsync(string schemaName, CancellationToken cancellationToken, params TableDefinition[] tables)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<object>> EvaluateAsync(string schemaName, CancellationToken cancellationToken, params TableDefinition[] tables)
        {
            throw new NotImplementedException();
        }
    }
}

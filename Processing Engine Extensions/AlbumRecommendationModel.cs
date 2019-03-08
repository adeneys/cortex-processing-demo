using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProcessingEngineDemo.XConnectExtensions;
using Sitecore.Processing.Engine.ML.Abstractions;
using Sitecore.Processing.Engine.Projection;
using Sitecore.XConnect;

namespace ProcessingEngineDemo.ProcessingEngineExtensions
{
    /// <summary>
    /// The custom processing model used when extracting data from xConnect.
    /// </summary>
    public class AlbumRecommendationModel : IModel<Contact>
    {
        /// <summary>
        /// The name of the options key containing the name of the table to project the data into.
        /// </summary>
        public const string OptionTableName = "tableName";

        /// <summary>
        /// The name of the table to project the data into.
        /// </summary>
        private readonly string _tableName;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="options">The options used to configure the model.</param>
        public AlbumRecommendationModel(IReadOnlyDictionary<string, string> options)
        {
            _tableName = options[OptionTableName];
        }

        /// <summary>
        /// The projection used to transform the xConnect data.
        /// </summary>
        public IProjection<Contact> Projection =>
            Sitecore.Processing.Engine.Projection.Projection.Of<Contact>().CreateTabular(
                _tableName,
                // Extract the artists of the albums which the contact has purchased.
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

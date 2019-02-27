using System;
using ProcessingEngineDemo.Common;
using Sitecore.XConnect;

namespace ProcessingEngineDemo.XConnectExtensions
{
    /// <summary>
    /// An <see cref="Outcome"/> triggered for a music purchase.
    /// </summary>
    public class MusicPurchaseOutcome : Outcome
    {
        /// <summary>
        /// The ID of the definition describing the outcome.
        /// </summary>
        public static readonly Guid DefaultDefinitionId = new Guid("{472628BE-CFB7-4F5B-8501-E2A7181910FC}");

        /// <summary>
        /// Gets or sets the album that was purchased.
        /// </summary>
        public AlbumDescriptor Album { get; set; }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="timestamp">The time and date when the outcome was triggered.</param>
        /// <param name="currencyCode">The currency code for the <paramref name="monetaryValue"/>.</param>
        /// <param name="monetaryValue">The value of the outcome.</param>
        public MusicPurchaseOutcome(DateTime timestamp, string currencyCode, decimal monetaryValue)
            : base(DefaultDefinitionId, timestamp, currencyCode, monetaryValue)
        {
        }
    }
}

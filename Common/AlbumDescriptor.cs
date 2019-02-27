namespace ProcessingEngineDemo.Common
{
    /// <summary>
    /// An album which has been purchased or recommended.
    /// </summary>
    public class AlbumDescriptor
    {
        /// <summary>
        /// Gets or sets the name of the artist that recorded the album.
        /// </summary>
        public string ArtistName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the artist that recorded the album.
        /// </summary>
        public string ArtistId { get; set; }

        /// <summary>
        /// Gets or sets the name of the album.
        /// </summary>
        public string AlbumName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the album.
        /// </summary>
        public string AlbumId { get; set; }
    }
}

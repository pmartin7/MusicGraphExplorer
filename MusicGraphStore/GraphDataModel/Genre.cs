using System.Collections.Generic;

namespace MusicGraphStore.GraphDataModel
{
    public class Genre
    {

        #region properties

        public string Name { get; set; }

        public List<Artist> Artists { get; set; }

        public List<Genre> RelatedGenres { get; set; }

        /// <summary>
        /// relevance is only set in context of the query,
        /// for instance in RelatedGenres,
        /// or in the Genres for an Artist
        /// </summary>
        public float Relevance { get; set; }

        #endregion

        #region constructor

        public Genre()
        {
            this.Artists = new List<Artist>();
            this.RelatedGenres = new List<Genre>();
        }

        #endregion

    }
}
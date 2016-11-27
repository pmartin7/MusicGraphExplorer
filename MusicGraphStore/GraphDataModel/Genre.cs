using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MusicGraphStore.GraphDataModel
{
    [DataContract]
    public class Genre
    {

        #region properties
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<Artist> Artists { get; set; }

        [DataMember]
        public List<Genre> RelatedGenres { get; set; }

        /// <summary>
        /// relevance is only set in context of the query,
        /// for instance in RelatedGenres,
        /// or in the Genres for an Artist
        /// </summary>
        [DataMember]
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
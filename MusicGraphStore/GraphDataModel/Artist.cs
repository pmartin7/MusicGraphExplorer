using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MusicGraphStore.GraphDataModel
{
    /// <summary>
    /// Artist Entity for the Graph Db
    /// </summary>
    [DataContract]
    public class Artist
    {

        #region Properties
        [DataMember]
        public string Name { get; set;}

        [DataMember]
        public int Popularity { get; set; }

        [DataMember]
        public string SpotifyId { get; set; }

        [DataMember]
        public string Url { get; set; }

        /// <summary>
        ///computed total genres from querying graph (may not match Genres.Count)
        /// </summary>
        [DataMember]
        public int TotalGenres { get; set; }

        /// <summary>
        ///Relevance is only populated when artist is returned in context of a query
        ///e.g. in the list of related artists for an artist
        ///or in the list of artists for a genre
        /// </summary>
        [DataMember]
        public float Relevance { get; set; }

        /// <summary>
        /// CommonGenres provide the number of common genres for a related artist
        /// This field is only present for a artist member of RelatedArtists
        /// </summary>
        [DataMember]
        public int CommonGenres { get; set; }

        [DataMember]
        public List<Artist> RelatedArtists { get; set; }

        [DataMember]
        public List<Genre> Genres { get; set; }

        #endregion

        #region Constructor
        public Artist()
        {
            RelatedArtists = new List<Artist>();
            Genres = new List<Genre>();
        }
        #endregion

    }
}

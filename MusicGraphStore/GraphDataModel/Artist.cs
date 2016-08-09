using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicGraphStore.GraphDataModel
{
    /// <summary>
    /// Artist Entity for the Graph Db
    /// </summary>
    public class Artist
    {

        #region Properties
        public string Name { get; set;}

        public string SpotifyId { get; set; }

        public int Popularity { get; set; }

        public string Url { get; set; }

        /// <summary>
        ///computed total genres from querying graph (may not match Genres.Count)
        /// </summary>
        public int TotalGenres { get; set; }

        /// <summary>
        ///Relevance is only populated when artist is returned in context of a query
        ///e.g. in the list of related artists for an artist
        ///or in the list of artists for a genre
        /// </summary>
        public float Relevance { get; set; }

        /// <summary>
        /// CommonGenres provide the number of common genres for a related artist
        /// This field is only present for a artist member of RelatedArtists
        /// </summary>
        public int CommonGenres { get; set; }

        public List<Artist> RelatedArtists { get; set; }

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

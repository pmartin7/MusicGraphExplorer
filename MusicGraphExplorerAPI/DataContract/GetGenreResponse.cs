using MusicGraphStore.GraphDataModel;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MusicGraphExplorerAPI.DataContract
{
    [DataContract]
    public class GetRelatedGenreResponse
    {

        #region properties
        [DataMember(Order = 0)]
        public string Name { get; set; }

        /// <summary>
        /// relevance is only set in context of the query,
        /// for instance in RelatedGenres,
        /// or in the Genres for an Artist
        /// </summary>
        [DataMember(Order = 1)]
        public float Relevance { get; set; }

        #endregion

        #region Contructors
        public GetRelatedGenreResponse(Genre genre)
        {
            this.Name = genre.Name;
            this.Relevance = genre.Relevance;
        }
        
        #endregion
    }
}
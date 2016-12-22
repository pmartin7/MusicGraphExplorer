using MusicGraphStore.GraphDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MusicGraphExplorerAPI.DataContract
{
    [DataContract]
    public class GetRelatedArtistResponse
    {
        #region Properties
        [DataMember(Order = 0)]
        public string Name { get; set; }

        /// <summary>
        ///Relevance is only populated when artist is returned in context of a query
        ///e.g. in the list of related artists for an artist
        ///or in the list of artists for a genre
        /// </summary>
        [DataMember(Order = 1)]
        public float Relevance { get; set; }

        [DataMember(Order = 2)]
        public int Popularity { get; set; }

        [DataMember(Order = 3)]
        public string SpotifyUrl { get; set; }

        [DataMember(Order = 4)]
        public string SpotifyId { get; set; }
        #endregion

        #region Constructor
        public GetRelatedArtistResponse(Artist artist)
        {
            //construct the list of Genres
            if (null != artist.Name) this.Name = artist.Name;
            this.Relevance = artist.Relevance;
            this.Popularity = artist.Popularity;
            if (null != artist.SpotifyId)
            {
                this.SpotifyId = artist.SpotifyId;
                this.SpotifyUrl = Helper.GetSpotifyUrlFromSpotifyId(this.SpotifyId);
            }
        }
        #endregion
    }
}